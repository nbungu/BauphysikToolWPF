using SQLite;

namespace BauphysikToolWPF.SQLiteRepo
{
    public enum OrientationType
    {
        North,
        NorthEast,
        East,
        SouthEast,
        South,
        SouthWest,
        West,
        NorthWest,
        Default = North
    }
    public class Orientation
    {
        //------Variablen-----//


        //------Eigenschaften-----//

        [PrimaryKey, NotNull, AutoIncrement, Unique]
        public int OrientationId { get; set; }
        [NotNull]
        public string TypeName { get; set; }

        //------Not part of the Database-----//

        [Ignore]
        public OrientationType Type
        {
            get
            {
                if (TypeName == null)
                    return OrientationType.Default;

                switch (TypeName)
                {
                    case "North":
                        return OrientationType.North;
                    case "North-East":
                        return OrientationType.NorthEast;
                    case "East":
                        return OrientationType.East;
                    case "South-East":
                        return OrientationType.SouthEast;
                    case "South":
                        return OrientationType.South;
                    case "South-West":
                        return OrientationType.SouthWest;
                    case "West":
                        return OrientationType.West;
                    case "North-West":
                        return OrientationType.NorthWest;
                    default:
                        return OrientationType.Default;
                }
            }
        }

        //------Konstruktor-----//

        // has to be default parameterless constructor when used as DB

        //------Methoden-----//
        public override string ToString() // Überschreibt/überlagert vererbte standard ToString() Methode 
        {
            return TypeName + " (Id: " + OrientationId + ")";
        }
    }
}
