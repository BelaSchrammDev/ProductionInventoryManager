using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage;

namespace IngameScript
{
    partial class Program
    {
        public class Assembler : IComparable<Assembler>
        {
            int BlueprintCount = 0;
            public List<AssemblerBluePrint> BlueprintList = new List<AssemblerBluePrint>();
            public Parameter parameter = new Parameter();
            public IMyAssembler AssemblerBlock;
            bool outputInventoryNotEmpty = false;
            bool IsSurvivalKit = false;
            bool RemoveItemMode = false;


            public Assembler(IMyAssembler a)
            {
                AssemblerBlock = a;
                IsSurvivalKit = a.BlockDefinition.TypeIdString == "SurvivalKit";
            }

            public bool BlockRemoved() { return AssemblerBlock.Closed; }


            public int CompareTo(Assembler other)
            {
                if (other.BlueprintCount < BlueprintCount) return 1;
                else if (other.BlueprintCount > BlueprintCount) return -1;
                return 0;
            }


            public void AddValidBlueprint(AssemblerBluePrint bluePrint)
            {
                if (BlockRemoved()) return;
                if (parameter.ControledByPIM() && AssemblerBlock.CanUseBlueprint(bluePrint.definition_id))
                {
                    bluePrint.o.Add(this);
                    bluePrint.NumBluePrintToAssembler++;
                    BlueprintList.Add(bluePrint);
                    BlueprintCount++;
                }
            }


            public bool AddBlueprintToQueue(AssemblerBluePrint bluePrint)
            {
                if (BlockRemoved()) return false;
                if (AssemblerBlock.Mode == MyAssemblerMode.Disassembly) return false;
                var ret = false;
                var bpmg = (bluePrint.MaximumItemAmount - bluePrint.CurrentItemAmount - bluePrint.AssemblyAmount);
                var mg = bpmg / bluePrint.NumBluePrintToAssembler;
                if (bpmg < 100)
                {
                    mg = bpmg;
                    ret = true;
                }
                AssemblerBlock.Repeating = false;
                try
                {
                    if (bluePrint.valid) AssemblerBlock.AddQueueItem(bluePrint.definition_id, (MyFixedPoint)mg);
                }
                catch (Exception e)
                {
                    bluePrint.valid = false;
                }
                return ret;
            }


            public void GetErrorInfo(StringBuilderExtended errString)
            {
                if (outputInventoryNotEmpty)
                {
                    errString.Append(parameter.Name);
                    errString.Append(" cannot unload output items.\n");
                }
                if (!BlockRemoved() && !AssemblerBlock.IsFunctional)
                {
                    errString.Append(parameter.Name);
                    errString.Append(" is damaged.\n");
                }
            }


            public void Refresh()
            {
                if (BlockRemoved()) return;
                var proditem_list = new List<MyProductionItem>();
                var bprint_list = new List<AssemblerBluePrint>();
                if (AssemblerBlock.Mode == MyAssemblerMode.Assembly)
                {
                    var outInventory = AssemblerBlock.GetInventory(1);
                    ClearInventory(outInventory);
                    outputInventoryNotEmpty = outInventory.CurrentVolume > 0;
                    AssemblerBlock.GetQueue(proditem_list);
                    for (int i = proditem_list.Count - 1; i >= 0; i--)
                    {
                        var bprint = AddProductionAmount(proditem_list[i]);
                        if (bprint != null)
                        {
                            bprint_list.Add(bprint);
                        }
                    }
                }
                else ClearInventory(AssemblerBlock.GetInventory(0));
                if (!parameter.ParseArgs(AssemblerBlock.CustomName, true)) return;
                BlueprintList.Clear();
                BlueprintCount = 0;
                if (AssemblerBlock.IsFunctional)
                {
                    if (AssemblerBlock.IsQueueEmpty)
                    {
                        if (!IsSurvivalKit)
                        {
                            if (!assemblers_off || parameter.IsParameter("Nooff")) AssemblerBlock.Enabled = true;
                            else AssemblerBlock.Enabled = false;
                        }
                        if (AssemblerBlock.Mode == MyAssemblerMode.Disassembly) ClearInventory(AssemblerBlock.GetInventory(1));
                        else ClearInventory(AssemblerBlock.GetInventory(0));
                    }
                    else
                    {
                        AssemblerBlock.Enabled = true;
                        if (AssemblerBlock.Mode == MyAssemblerMode.Assembly)
                        {
                            if (RemoveItemMode)
                            {
                                RemoveItemMode = false;
                                if (delete_queueItem_if_max)
                                {
                                    for (int i = proditem_list.Count - 1; i >= 0; i--)
                                    {
                                        var productionItem = proditem_list[i];
                                        foreach (var bprint in bprint_list)
                                        {
                                            if (bprint.definition_id.SubtypeName == productionItem.BlueprintId.SubtypeName
                                                && bprint.MaximumItemAmount != 0
                                                && bprint.MaximumItemAmount <= bprint.CurrentItemAmount)
                                            {
                                                AssemblerBlock.RemoveQueueItem(i, productionItem.Amount);
                                                proditem_list.RemoveAt(i);
                                            }
                                        }

                                    }
                                }
                            }
                            else
                            {
                                RemoveItemMode = true;
                                AssemblerBluePrint firstBlueprint = proditem_list.Count > 1 ? GetBluePrintByProductionItem(proditem_list[0]) : null;
                                if (firstBlueprint != null)
                                {
                                    for (int i = proditem_list.Count - 1; i > 0; i--)
                                    {
                                        var productionItemBlueprint = GetBluePrintByProductionItem(proditem_list[i]);
                                        if (productionItemBlueprint != null
                                            && productionItemBlueprint.MaximumItemAmount != 0
                                            && productionItemBlueprint.ItemPriority > firstBlueprint.ItemPriority)
                                        {
                                            // Move
                                            AssemblerBlock.MoveQueueItemRequest(proditem_list[i].ItemId, 0);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}
