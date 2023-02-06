using BauphysikToolWPF.SQLiteRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BauphysikToolWPF.ComponentCalculations
{
    public class ElementRequirements
    {
        enum BuildingUsage
        {
            NonResidential, // 0, Nichtwohngebäude
            Residential     // 1, Wohngebäude
        }
        enum BuildingAge
        {
            Existing,       // 0, Bestandsgebäude
            New             // 1, Neubau
        }
        enum EnvCondition
        {
            TiGreater19,    // 0, ValueA
            TiBetween12And19// 1, ValueB
        }

        public bool CheckUValue(double u_value)
        {
            //double requiredValue = DatabaseAccess.QueryConstructionTypeById();
            return true;
        }


        //TODO: Add GEG/DIN Requirements
    }
}
