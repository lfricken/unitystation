using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class WireConnect : ElectricalOIinheritance
{
	public CableInheritance ControllingCable;
	public CableLine RelatedLine;

	public override void DirectionInput( GameObject SourceInstance, ElectricalOIinheritance ComingFrom, CableLine PassOn  = null){
		InputOutputFunctions.DirectionInput ( SourceInstance, ComingFrom, this);
		if (PassOn == null) {
			if (RelatedLine != null) {
				if (RelatedLine.TheEnd == this.GetComponent<ElectricalOIinheritance> ()) {
					//Logger.Log ("looc");
				} else if (RelatedLine.TheStart == this.GetComponent<ElectricalOIinheritance> ()) {
					//Logger.Log ("cool");
				} else {
					//Logger.Log ("hELP{!!!");
				}
			} else {
				if (!(Data.connections.Count > 2)) {
					RelatedLine = new CableLine ();
					if (RelatedLine == null) {
						Logger.Log("DirectionInput: RelatedLine is null.", Category.Power);
					}
					RelatedLine.InitialGenerator = SourceInstance;
					RelatedLine.TheStart = this;
					lineExplore (RelatedLine, SourceInstance);
					if (RelatedLine.TheEnd == null) {
						RelatedLine = null;
					}
				}
			}
		}
	}

	public override void DirectionOutput( GameObject SourceInstance){
		
		int SourceInstanceID = SourceInstance.GetInstanceID();
		//Logger.Log (this.gameObject.GetInstanceID().ToString() + " <ID | Downstream = "+Data.Downstream[SourceInstanceID].Count.ToString() + " Upstream = " + Data.Upstream[SourceInstanceID].Count.ToString () + this.name + " <  name! ", Category.Electrical);
		if (RelatedLine == null) {
			InputOutputFunctions.DirectionOutput (SourceInstance, this);
		} else {
			
			ElectricalOIinheritance GoingTo; 
			if (RelatedLine.TheEnd == this.GetComponent<ElectricalOIinheritance> ()) {
				GoingTo = RelatedLine.TheStart;
			} else if (RelatedLine.TheStart == this.GetComponent<ElectricalOIinheritance> ()) {
				GoingTo = RelatedLine.TheEnd;
			} else {
				GoingTo = null;
				return; 
			}

			if (GoingTo != null) {
				//Logger.Log ("to" + GoingTo.GameObject ().name);///wow
			}

			foreach (ElectricalOIinheritance bob in Data.SupplyDependent[SourceInstanceID].Upstream){
				//Logger.Log("Upstream" + bob.GameObject ().name );
			}
			if (!(Data.SupplyDependent[SourceInstanceID].Downstream.Contains (GoingTo) || Data.SupplyDependent[SourceInstanceID].Upstream.Contains (GoingTo)) )  {
				Data.SupplyDependent[SourceInstanceID].Downstream.Add (GoingTo);

				RelatedLine.DirectionInput (SourceInstance, this);
			} else {
				InputOutputFunctions.DirectionOutput (SourceInstance, this,RelatedLine);
			}
		}
	}

	public override void ElectricityOutput(float Current, GameObject SourceInstance)
	{
		//Logger.Log (Current.ToString () + " How much current", Category.Electrical);
		InputOutputFunctions.ElectricityOutput(Current, SourceInstance, this);
		if (ControllingCable != null) {
			ElectricalSynchronisation.CableUpdates.Add(ControllingCable);
		}
	}

	//public void UpdateRelatedLine()
	//{
	//	if (RelatedLine != null)
	//	{
	//		RelatedLine.UpdateCoveringCable();

	//	}
	//}

	public void lineExplore(CableLine PassOn, GameObject SourceInstance = null)
	{

		if (Data.connections.Count <= 0)
		{
			FindPossibleConnections();
		}

		if (!(Data.connections.Count > 2))
		{
			RelatedLine = PassOn;
			if (!(this == PassOn.TheStart))
			{
				if (PassOn.TheEnd != null)
				{
					PassOn.Covering.Add(PassOn.TheEnd);
					PassOn.TheEnd = this;
				}
				else
				{
					PassOn.TheEnd = this;
				}
			}
			foreach (ElectricalOIinheritance Related in Data.connections)
			{
				if (!(RelatedLine.Covering.Contains(Related) || RelatedLine.TheStart == Related))
				{
					bool canpass = true;
					if (SourceInstance != null)
					{
						int SourceInstanceID = SourceInstance.GetInstanceID();
						if (Data.SupplyDependent[SourceInstanceID].Upstream.Contains(Related))
						{
							canpass = false;
						}
					}
					if (canpass)
					{
						if (Related.GameObject().GetComponent<WireConnect>() != null)
						{
							Related.GameObject().GetComponent<WireConnect>().lineExplore(RelatedLine);
						}
					}

				}
			}
		}
	}


	public override void FlushConnectionAndUp()
	{

		ElectricalDataCleanup.PowerSupplies.FlushConnectionAndUp(this);
		RelatedLine = null;
		//InData.ControllingDevice.PotentialDestroyed();
	}
	public override void FlushResistanceAndUp(GameObject SourceInstance = null)
	{
		//TODO: yeham, Might need to work on in future but not used Currently
		ElectricalDataCleanup.PowerSupplies.FlushResistanceAndUp(this, SourceInstance);
	}
	public override void FlushSupplyAndUp(GameObject SourceInstance = null)
	{
		bool SendToline = false;
		if (RelatedLine != null)
		{
			if (SourceInstance == null)
			{
				foreach (var Supply in Data.SupplyDependent)
				{
					if (Supply.Value.CurrentComingFrom.Count > 0 || Supply.Value.CurrentGoingTo.Count > 0)
					{
						SendToline = true;
					}
				}
			}
			else
			{
				int InstanceID = SourceInstance.GetInstanceID();
				if (Data.SupplyDependent.ContainsKey(InstanceID))
				{
					if (Data.SupplyDependent[InstanceID].CurrentComingFrom.Count > 0)
					{
						SendToline = true;
					}
					else if (Data.SupplyDependent[InstanceID].CurrentGoingTo.Count > 0)
					{
						SendToline = true;
					}
				}
			}
		}
		ElectricalDataCleanup.PowerSupplies.FlushSupplyAndUp(this, SourceInstance);
		if (SendToline)
		{
			RelatedLine.PassOnFlushSupplyAndUp(this, SourceInstance);
		}
	}
	public override void RemoveSupply(GameObject SourceInstance = null)
	{
		bool SendToline = false;
		if (RelatedLine != null)
		{
			if (SourceInstance == null)
			{
				foreach (var Supply in Data.SupplyDependent)
				{
					if (Supply.Value.Downstream.Count > 0 || Supply.Value.Upstream.Count > 0)
					{
						SendToline = true;
					}
				}
			}
			else
			{
				int InstanceID = SourceInstance.GetInstanceID();
				if (Data.SupplyDependent[InstanceID].Downstream.Count > 0 || Data.SupplyDependent[InstanceID].Upstream.Count > 0)
				{
					SendToline = true;
				}
			}
		}
		ElectricalDataCleanup.PowerSupplies.RemoveSupply(this, SourceInstance);
		if (SendToline){
			RelatedLine.PassOnRemoveSupply (this, SourceInstance);
		}
	}
	[ContextMethod("Details", "Magnifying_glass")]
	public override void ShowDetails()
	{
		if (isServer)
		{
			ElectricityFunctions.WorkOutActualNumbers(this);
			Logger.Log("connections " + (string.Join(",", Data.connections)), Category.Electrical);
			Logger.Log("ID " + (this.GetInstanceID()), Category.Electrical);
			Logger.Log("Type " + (InData.Categorytype.ToString()), Category.Electrical);
			Logger.Log("Can connect to " + (string.Join(",", InData.CanConnectTo)), Category.Electrical);
			foreach (var Supply in Data.SupplyDependent) {
				string ToLog;
				ToLog = "Supply > " + Supply.Key + "\n";
				ToLog += "Upstream > ";
				ToLog += string.Join(",", Supply.Value.Upstream) + "\n";
				ToLog += "Downstream > ";
				ToLog += string.Join(",", Supply.Value.Downstream) + "\n";
				ToLog += "ResistanceGoingTo > ";
				ToLog += string.Join(",", Supply.Value.ResistanceGoingTo) + "\n";
				ToLog += "ResistanceComingFrom > ";
				ToLog += string.Join(",", Supply.Value.ResistanceComingFrom) + "\n";
				ToLog += "CurrentComingFrom > ";
				ToLog += string.Join(",", Supply.Value.CurrentComingFrom) + "\n";
				ToLog += "CurrentGoingTo > ";
				ToLog += string.Join(",", Supply.Value.CurrentGoingTo) + "\n";
				ToLog += Supply.Value.SourceVoltages.ToString();
				Logger.Log(ToLog, Category.Electrical);
			}
			if (RelatedLine != null)
			{
				Logger.Log("line heree!!!");
				ElectricityFunctions.WorkOutActualNumbers(RelatedLine.TheEnd);
				Data.ActualVoltage = RelatedLine.TheEnd.Data.ActualVoltage;
				Data.CurrentInWire = RelatedLine.TheEnd.Data.CurrentInWire;
				Data.EstimatedResistance = RelatedLine.TheEnd.Data.EstimatedResistance;
				foreach (var Supply in RelatedLine.TheEnd.Data.SupplyDependent)
				{
					string ToLog;
					ToLog = "Supply > " + Supply.Key + "\n";
					ToLog += "Upstream > ";
					ToLog += string.Join(",", Supply.Value.Upstream) + "\n";
					ToLog += "Downstream > ";
					ToLog += string.Join(",", Supply.Value.Downstream) + "\n";
					ToLog += "ResistanceGoingTo > ";
					ToLog += string.Join(",", Supply.Value.ResistanceGoingTo) + "\n";
					ToLog += "ResistanceComingFrom > ";
					ToLog += string.Join(",", Supply.Value.ResistanceComingFrom) + "\n";
					ToLog += "CurrentComingFrom > ";
					ToLog += string.Join(",", Supply.Value.CurrentComingFrom) + "\n";
					ToLog += "CurrentGoingTo > ";
					ToLog += string.Join(",", Supply.Value.CurrentGoingTo) + "\n";
					ToLog += Supply.Value.SourceVoltages.ToString();
					Logger.Log(ToLog, Category.Electrical);
				}
			}
			Logger.Log(" ActualVoltage > " + Data.ActualVoltage + " CurrentInWire > " + Data.CurrentInWire + " EstimatedResistance >  " + Data.EstimatedResistance, Category.Electrical);
		}

		RequestElectricalStats.Send(PlayerManager.LocalPlayer, gameObject);
	}
}