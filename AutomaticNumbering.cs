using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoNumberRoom
{
    [Transaction(TransactionMode.Manual)]
    public class AutomaticNumbering : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var doc = commandData.Application.ActiveUIDocument.Document;

            try
            {
                List<Room> rooms = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Rooms)
                    .WhereElementIsNotElementType()
                    .Cast<Room>()
                    .ToList();

                if (rooms == null)
                {
                    TaskDialog.Show("Ошибка", "Помещения не найдены");
                    return Result.Cancelled;
                }

                SortRooms(rooms);

                using (var ts = new Transaction(doc, "Create numbers"))
                {
                    ts.Start();
                    for (int i = 0; i < rooms.Count; i++)
                    {
                        rooms[i].Number = (i + 1).ToString();
                    }
                    ts.Commit();
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        private void SortRooms(List<Room> rooms)
        {
            LocationPoint tmpPoint = null;
            LocationPoint roomPoint = null;
            Room listRoom = null;
            int result = 0;
            int amount = rooms.Count;
            bool flag = false;

            for (int i = 0; i < amount - 1; i++)
            {
                Room tmpRoom = rooms[i];
                for (int j = i + 1; j < amount; j++)
                {
                    tmpPoint = tmpRoom.Location as LocationPoint;
                    listRoom = rooms[j];
                    roomPoint = listRoom.Location as LocationPoint;

                    if (null == tmpPoint || null == roomPoint)
                    {
                        return;
                    }

                    if (tmpPoint.Point.Z > roomPoint.Point.Z)
                    {
                        tmpRoom = listRoom;
                        result = j;

                        flag = true;
                    }
                    else if (tmpPoint.Point.Z == roomPoint.Point.Z)
                    {
                        if (tmpPoint.Point.X > roomPoint.Point.X)
                        {
                            tmpRoom = listRoom;
                            result = j;
                            flag = true;
                        }
                        else if (tmpPoint.Point.X == roomPoint.Point.X &&
                                 tmpPoint.Point.Y > roomPoint.Point.Y)
                        {
                            tmpRoom = listRoom;
                            result = j;
                            flag = true;
                        }
                    }
                }

                if (flag)
                {
                    Room tempRoom = rooms[i];
                    rooms[i] = rooms[result];
                    rooms[result] = tempRoom;
                }
            }
        }
    }
}
