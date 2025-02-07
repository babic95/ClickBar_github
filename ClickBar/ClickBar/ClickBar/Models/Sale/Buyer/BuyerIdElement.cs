using ClickBar.Enums.Sale.Buyer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar.Models.Sale.Buyer
{
    public class BuyerIdElement : ObservableObject
    {
        private int _id;
        private string _description;

        public BuyerIdElement(BuyerIdElementEnumeration id)
        {
            switch (id)
            {
                case BuyerIdElementEnumeration.PIB:
                    Description = "10:PIB";
                    Id = 10;
                    break;
                case BuyerIdElementEnumeration.JMBG:
                    Description = "11:JMBG";
                    Id = 11;
                    break;
                case BuyerIdElementEnumeration.PIB_JBKJS:
                    Description = "12:PIB i JBKJS";
                    Id = 12;
                    break;
                case BuyerIdElementEnumeration.PENZIONERI:
                    Description = "13:Kod penzionerske kartice";
                    Id = 13;
                    break;
                case BuyerIdElementEnumeration.PIB_PG:
                    Description = "14:PIB poljprivrednog gazdinstva";
                    Id = 14;
                    break;
                case BuyerIdElementEnumeration.JMBG_PG:
                    Description = "15:JMBG poljoprivrednog gazdinstva";
                    Id = 15;
                    break;
                case BuyerIdElementEnumeration.BPG:
                    Description = "16:Broj poljoprivrednog gazdinstva";
                    Id = 16;
                    break;
                case BuyerIdElementEnumeration.BrojLicneKarte:
                    Description = "20:Broj lične karte";
                    Id = 20;
                    break;
                case BuyerIdElementEnumeration.BrojIzbeglickeLegitimacije:
                    Description = "21:Broj izbegličke legitimacije";
                    Id = 21;
                    break;
                case BuyerIdElementEnumeration.EBS:
                    Description = "22:EBS";
                    Id = 22;
                    break;
                case BuyerIdElementEnumeration.BrojPasosaDomaceFizickoLice:
                    Description = "23:Broj pasoša (domaće fizicko lice)";
                    Id = 23;
                    break;
                case BuyerIdElementEnumeration.BrojPasosaStranoFizickoLice:
                    Description = "30:Broj pasoša (strano fizicko lice)";
                    Id = 30;
                    break;
                case BuyerIdElementEnumeration.BrojDiplomatskeLegitimacije:
                    Description = "31:Broj diplomatske legitimacije";
                    Id = 31;
                    break;
                case BuyerIdElementEnumeration.BrojLicneKarteMKD:
                    Description = "32:Broj lične karte - Severna Makedonija";
                    Id = 32;
                    break;
                case BuyerIdElementEnumeration.BrojLicneKarteMNE:
                    Description = "33:Broj lične karte - Crna Gora";
                    Id = 33;
                    break;
                case BuyerIdElementEnumeration.BrojLicneKarteALB:
                    Description = "34:Broj lične karte - Albanija";
                    Id = 34;
                    break;
                case BuyerIdElementEnumeration.BrojLicneKarteBIH:
                    Description = "35:Broj lične karte - Bosna i Hercegovina";
                    Id = 35;
                    break;
                case BuyerIdElementEnumeration.BrojLicneKarteEU:
                    Description = "36:Strano fizičko lice iz EU";
                    Id = 36;
                    break;
                case BuyerIdElementEnumeration.PoreskiIdIzStraneDrzave:
                    Description = "40:Poreski ID iz strane države (TIN)";
                    Id = 40;
                    break;
            }
        }
        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChange(nameof(Id));
            }
        }
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChange(nameof(Description));
            }
        }
    }
}
