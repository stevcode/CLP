using System;
using System.Linq;
using CLP.Entities;
using Emily = CLP.Entities.Old;

namespace Classroom_Learning_Partner.Services
{
    public class ConversionService
    {
        public ConversionService() { }

        #region Emily Conversions

        public Person ConvertPerson(Emily.Person person)
        {
            var newPerson = new Person();
            if (person == null)
            {
                Console.WriteLine("[CONVERSION ERROR]: Old Person is null.");
                return newPerson;
            }

            newPerson.ID = person.ID;
            newPerson.Alias = person.Alias;
            newPerson.IsStudent = person.IsStudent;

            if (string.IsNullOrWhiteSpace(person.FullName))
            {
                Console.WriteLine("[CONVERSION ERROR]: Person.FullName is blank.");
                return newPerson;
            }

            var nameParts = person.FullName.Split(' ').ToList();
            if (!nameParts.Any())
            {
                Console.WriteLine("[CONVERSION ERROR]: Person.FullName is blank.");
                return newPerson;
            }

            var firstName = nameParts.First();
            nameParts.RemoveAt(0);
            if (!nameParts.Any())
            {
                newPerson.FirstName = firstName;
                return newPerson;
            }

            var lastName = nameParts.Last();
            nameParts.RemoveAt(nameParts.Count - 1);
            if (!nameParts.Any())
            {
                newPerson.FirstName = firstName;
                newPerson.LastName = lastName;
                return newPerson;
            }

            var middleName = string.Join(" ", nameParts);

            newPerson.FirstName = firstName;
            newPerson.LastName = lastName;
            newPerson.MiddleName = middleName;
            return newPerson;
        }

        public Notebook ConvertNotebook(Emily.Notebook notebook)
        {
            var newPerson = ConvertPerson(notebook.Owner);

            var newNotebook = new Notebook
                              {
                                  ID = notebook.ID,
                                  Owner = newPerson,
                                  Name = notebook.Name,
                                  CreationDate = notebook.CreationDate,
                                  LastSavedDate = notebook.LastSavedDate,
                                  CurrentPageID = notebook.CurrentPageID,
                                  CurrentPageVersionIndex = notebook.CurrentPageVersionIndex
                              };

            return newNotebook;
        }

        public CLPPage ConvertPage(Emily.CLPPage page)
        {
            var newPerson = ConvertPerson(page.Owner);

            var newPage = new CLPPage
                          {
                              ID = page.ID,
                              Owner = newPerson,
                              PageNumber = page.PageNumber,
                              DifferentiationLevel = page.DifferentiationLevel,
                              VersionIndex = page.VersionIndex,
                              LastVersionIndex = page.LastVersionIndex,
                              CreationDate = page.CreationDate,
                              PageType = page.PageType == Emily.PageTypes.Animation ? PageTypes.Animation : PageTypes.Default,
                              SubmissionType = page.SubmissionType == Emily.SubmissionTypes.Single ? SubmissionTypes.Single : SubmissionTypes.Unsubmitted,
                              SubmissionTime = page.SubmissionTime,
                              Height = page.Height,
                              Width = page.Width,
                              InitialAspectRatio = page.InitialAspectRatio
                          };

            // TODO: Convert History
            // TODO: Tags

            foreach (var serializedStroke in page.SerializedStrokes)
            {
                var stroke = serializedStroke.ToStroke();
                newPage.InkStrokes.Add(stroke);
            }

            foreach (var pageObject in page.PageObjects)
            {
                var newPageObject = ConverPageObject(pageObject, newPage);
                newPage.PageObjects.Add(newPageObject);
                var divisionTemplate = newPageObject as DivisionTemplate;
                if (divisionTemplate != null &&
                    divisionTemplate.RemainderTiles != null)
                {
                    newPage.PageObjects.Add(divisionTemplate.RemainderTiles);
                }
            }

            return newPage;
        }

        public IPageObject ConverPageObject(Emily.IPageObject pageObject, CLPPage newPage)
        {
            IPageObject newPageObject = null;

            TypeSwitch.On(pageObject).Case<Emily.Shape>(p =>
                                                        {
                                                            newPageObject = ConvertShape(p, newPage);
                                                        })
                                     .Case<Emily.CLPTextBox>(p =>
                                                             {
                                                                 newPageObject = ConvertTextBox(p, newPage);
                                                             })
                                     .Case<Emily.CLPImage>(p =>
                                                           {
                                                               newPageObject = ConvertImage(p, newPage);
                                                           })
                                     .Case<Emily.CLPArray>(p =>
                                                           {
                                                               newPageObject = ConvertArray(p, newPage);
                                                           })
                                     .Case<Emily.FuzzyFactorCard>(p =>
                                                                  {
                                                                      newPageObject = ConvertDivisionTemplate(p, newPage);
                                                                  });

            return newPageObject;
        }

        public Shape ConvertShape(Emily.Shape shape, CLPPage newPage)
        {
            var newShape = new Shape
                           {
                               ID = shape.ID,
                               XPosition = shape.XPosition,
                               YPosition = shape.YPosition,
                               Height = shape.Height,
                               Width = shape.Width,
                               OwnerID = shape.OwnerID,
                               CreatorID = shape.CreatorID,
                               CreationDate = shape.CreationDate,
                               PageObjectFunctionalityVersion = "Emily5.22.2014",
                               IsManipulatableByNonCreator = shape.IsManipulatableByNonCreator,
                               ParentPage = newPage
                           };

            switch (shape.ShapeType)
            {
                case Emily.ShapeType.Rectangle:
                    newShape.ShapeType = ShapeType.Rectangle;
                    break;
                case Emily.ShapeType.Ellipse:
                    newShape.ShapeType = ShapeType.Ellipse;
                    break;
                case Emily.ShapeType.Triangle:
                    newShape.ShapeType = ShapeType.Triangle;
                    break;
                case Emily.ShapeType.HorizontalLine:
                    newShape.ShapeType = ShapeType.HorizontalLine;
                    break;
                case Emily.ShapeType.VerticalLine:
                    newShape.ShapeType = ShapeType.VerticalLine;
                    break;
                case Emily.ShapeType.Protractor:
                    newShape.ShapeType = ShapeType.Protractor;
                    break;
                default:
                    newShape.ShapeType = ShapeType.Rectangle;
                    break;
            }

            return newShape;
        }

        public CLPTextBox ConvertTextBox(Emily.CLPTextBox textBox, CLPPage newPage)
        {
            var newTextBox = new CLPTextBox
                             {
                                 ID = textBox.ID,
                                 XPosition = textBox.XPosition,
                                 YPosition = textBox.YPosition,
                                 Height = textBox.Height,
                                 Width = textBox.Width,
                                 OwnerID = textBox.OwnerID,
                                 CreatorID = textBox.CreatorID,
                                 CreationDate = textBox.CreationDate,
                                 PageObjectFunctionalityVersion = "Emily5.22.2014",
                                 IsManipulatableByNonCreator = textBox.IsManipulatableByNonCreator,
                                 ParentPage = newPage
                             };
            newTextBox.Text = textBox.Text;

            return newTextBox;
        }

        public CLPImage ConvertImage(Emily.CLPImage image, CLPPage newPage)
        {
            var newImage = new CLPImage
                           {
                               ID = image.ID,
                               XPosition = image.XPosition,
                               YPosition = image.YPosition,
                               Height = image.Height,
                               Width = image.Width,
                               OwnerID = image.OwnerID,
                               CreatorID = image.CreatorID,
                               CreationDate = image.CreationDate,
                               PageObjectFunctionalityVersion = "Emily5.22.2014",
                               IsManipulatableByNonCreator = image.IsManipulatableByNonCreator,
                               ParentPage = newPage
                           };
            newImage.ImageHashID = image.ImageHashID;

            return newImage;
        }

        public RemainderTiles ConvertRemainderTiles(Emily.RemainderTiles remainderTiles, CLPPage newPage)
        {
            var newRemainderTiles = new RemainderTiles
                                    {
                                        ID = remainderTiles.ID,
                                        XPosition = remainderTiles.XPosition,
                                        YPosition = remainderTiles.YPosition,
                                        Height = remainderTiles.Height,
                                        Width = remainderTiles.Width,
                                        OwnerID = remainderTiles.OwnerID,
                                        CreatorID = remainderTiles.CreatorID,
                                        CreationDate = remainderTiles.CreationDate,
                                        PageObjectFunctionalityVersion = "Emily5.22.2014",
                                        IsManipulatableByNonCreator = remainderTiles.IsManipulatableByNonCreator,
                                        ParentPage = newPage
                                    };
            newRemainderTiles.TileColors = remainderTiles.TileOffsets;

            return newRemainderTiles;
        }

        public CLPArray ConvertArray(Emily.CLPArray array, CLPPage newPage)
        {
            var newArray = new CLPArray
                           {
                               ID = array.ID,
                               XPosition = array.XPosition,
                               YPosition = array.YPosition,
                               Height = array.Height,
                               Width = array.Width,
                               OwnerID = array.OwnerID,
                               CreatorID = array.CreatorID,
                               CreationDate = array.CreationDate,
                               PageObjectFunctionalityVersion = "Emily5.22.2014",
                               IsManipulatableByNonCreator = array.IsManipulatableByNonCreator,
                               ParentPage = newPage,
                               Rows = array.Rows,
                               Columns = array.Columns,
                               IsGridOn = array.IsGridOn,
                               IsDivisionBehaviorOn = array.IsDivisionBehaviorOn,
                               IsSnappable = array.IsSnappable,
                               IsTopLabelVisible = array.IsTopLabelVisible,
                               IsSideLabelVisible = array.IsSideLabelVisible,
                               CanAcceptStrokes = false
                           };

            switch (array.ArrayType)
            {
                case Emily.ArrayTypes.Array:
                    newArray.ArrayType = ArrayTypes.Array;
                    break;
                case Emily.ArrayTypes.ArrayCard:
                    newArray.ArrayType = ArrayTypes.ArrayCard;
                    break;
                case Emily.ArrayTypes.FactorCard:
                    newArray.ArrayType = ArrayTypes.FactorCard;
                    break;
                default:
                    newArray.ArrayType = ArrayTypes.Array;
                    break;
            }

            foreach (var division in array.HorizontalDivisions)
            {
                var newDivision = ConvertArrayDivision(division);
                newArray.HorizontalDivisions.Add(newDivision);
            }

            foreach (var division in array.VerticalDivisions)
            {
                var newDivision = ConvertArrayDivision(division);
                newArray.VerticalDivisions.Add(newDivision);
            }

            return newArray;
        }

        public CLPArrayDivision ConvertArrayDivision(Emily.CLPArrayDivision division)
        {
            var newDivision = new CLPArrayDivision
                              {
                                  Position = division.Position,
                                  Length = division.Length,
                                  Value = division.Value,
                                  Orientation = division.Orientation == Emily.ArrayDivisionOrientation.Horizontal ? ArrayDivisionOrientation.Horizontal : ArrayDivisionOrientation.Vertical
                              };

            return newDivision;
        }

        public DivisionTemplate ConvertDivisionTemplate(Emily.FuzzyFactorCard ffc, CLPPage newPage)
        {
            var newDivisionTemplate = new DivisionTemplate
                                      {
                                          ID = ffc.ID,
                                          XPosition = ffc.XPosition,
                                          YPosition = ffc.YPosition,
                                          Height = ffc.Height,
                                          Width = ffc.Width,
                                          OwnerID = ffc.OwnerID,
                                          CreatorID = ffc.CreatorID,
                                          CreationDate = ffc.CreationDate,
                                          PageObjectFunctionalityVersion = "Emily5.22.2014",
                                          IsManipulatableByNonCreator = ffc.IsManipulatableByNonCreator,
                                          ParentPage = newPage,
                                          Rows = ffc.Rows,
                                          Columns = ffc.Columns,
                                          IsGridOn = ffc.IsGridOn,
                                          IsDivisionBehaviorOn = ffc.IsDivisionBehaviorOn,
                                          IsSnappable = ffc.IsSnappable,
                                          IsTopLabelVisible = ffc.IsTopLabelVisible,
                                          IsSideLabelVisible = ffc.IsSideLabelVisible,
                                          Dividend = ffc.Dividend
                                      };

            if (ffc.RemainderTiles != null)
            {
                var newRemainderTiles = ConvertRemainderTiles(ffc.RemainderTiles, newPage);
                newDivisionTemplate.RemainderTiles = newRemainderTiles;
                newDivisionTemplate.IsRemainderTilesVisible = true;
            }

            foreach (var division in ffc.HorizontalDivisions)
            {
                var newDivision = ConvertArrayDivision(division);
                newDivisionTemplate.HorizontalDivisions.Add(newDivision);
            }

            foreach (var division in ffc.VerticalDivisions)
            {
                var newDivision = ConvertArrayDivision(division);
                newDivisionTemplate.VerticalDivisions.Add(newDivision);
            }

            return newDivisionTemplate;
        }

        #endregion // Emily Conversions
    }
}
