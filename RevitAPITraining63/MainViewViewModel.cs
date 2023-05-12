using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Prism.Commands;
using RevitAPITrainingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace RevitAPITraining63
{
    public class MainViewViewModel
    {
        private ExternalCommandData _commandData;
        private readonly object SelectedFamilyInstance;

        public FamilyInstance FamilyInstance { get; }
        public DelegateCommand SaveCommand { get; }
        
        public List<XYZ> Points { get; } = new List<XYZ>();       
        
        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            FamilyInstance = FamilyInstanceUtils.GetFamilyInstance(commandData);
            
            SaveCommand = new DelegateCommand(OnSaveCommand);
           
            Points = SelectionUtils.GetPoints(_commandData, "Выберите точки", ObjectSnapTypes.Endpoints);
        }

        private void OnSaveCommand()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            if (Points.Count < 2)
                return;

            var curves = new List<Curve>();
            for (int i = 0; i < Points.Count; i++)
            {
                if (i == 0)
                    continue;


                var prevPoint = Points[i - 1];
                var currentPoint = Points[i];
                Curve curve = Line.CreateBound(prevPoint, currentPoint);
                curves.Add(curve);
            }

            using (var ts = new Transaction(doc, "Create duct"))
            {
                ts.Start();
                foreach (var curve in curves)
                {

                    FamilyInstance.Create(doc, curves,SelectedFamilyInstance, UnitUtils.ConvertToInternalUnits, 0, false, false);
                }
                ts.Commit();
            }

            RaiseCloseRequest();
        }

        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}

