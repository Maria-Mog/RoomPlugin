using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomPlugin
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class RoomPlugin : IExternalCommand

    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            
            // Создать комнату, выбрав точку на виде
            bool selection = true;
            while (selection)
            {
                try
                {
                    XYZ point = uidoc.Selection.PickPoint("Щелкните для размещения помещения");

                    Transaction ts = new Transaction(doc, "Расстановка помещений");
                    ts.Start();

                    Room newRoom = doc.Create.NewRoom(doc.ActiveView.GenLevel, new UV(point.X, point.Y));
                    
                    string a = $"{newRoom.Level.Name}";
                    int value;
                    int.TryParse(string.Join("", a.Where(c => char.IsDigit(c))), out value);

                    string roomName = $"{value}_{newRoom.Number}";
                    XYZ centerRoom = GetRoomCenter(newRoom);

                    RoomTag tag = doc.Create.NewRoomTag(new LinkElementId(newRoom.Id), new UV(centerRoom.X, centerRoom.Y), null);

                    newRoom.get_Parameter(BuiltInParameter.ROOM_NAME).Set(roomName);
                  //  newRoom.get_Parameter(BuiltInParameter.ROOM_NUMBER).Set(roomNumber);
                    ts.Commit();
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    selection = false;
                    break;
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                    selection = false;
                    break;
                }
            }            
            return Result.Succeeded;
        }
        public XYZ GetRoomCenter(Room room)
        {
            XYZ boundCenter = GetElementCenter(room);
            LocationPoint loc = (LocationPoint)room.Location;
            XYZ roomCenter = new XYZ(boundCenter.X, boundCenter.Y, loc.Point.Z);
            return roomCenter;
        }
        public XYZ GetElementCenter(Element element)
        {
            BoundingBoxXYZ boundingBox = element.get_BoundingBox(null);
            return (boundingBox.Max + boundingBox.Min) / 2;
        }
    }
}
