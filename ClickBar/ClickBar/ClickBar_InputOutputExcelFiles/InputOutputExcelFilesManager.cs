using ClosedXML.Excel;
using ClickBar_InputOutputExcelFiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClickBar_DatabaseSQLManager.Models;
using ClickBar_Settings;
using ClickBar_DatabaseSQLManager;
using ClickBar_Common.Enums;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ClickBar_InputOutputExcelFiles
{
    public sealed class InputOutputExcelFilesManager
    {
        #region Fields Singleton
        private static readonly object lockObject = new object();
        private static InputOutputExcelFilesManager instance = null;
        #endregion Fields Singleton

        #region Fields
        #endregion Fields

        #region Constructors
        private InputOutputExcelFilesManager() { }
        public static InputOutputExcelFilesManager Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new InputOutputExcelFilesManager();
                    }
                    return instance;
                }
            }
        }
        #endregion Constructors

        #region Public methods
        public async Task<List<SupergroupDB>?> ImportSupergroups(SqlServerDbContext sqliteDbContext)
        {
            string? path = SettingsManager.Instance.GetPathToImportSupergroups();

            if (!string.IsNullOrEmpty(path))
            {

                List<SupergroupExcel> excelSuperoups = ImportExcel<SupergroupExcel>(path, "Nadgrupe");

                List<SupergroupDB> supergroups = new List<SupergroupDB>();

                excelSuperoups.ForEach(async supergroup =>
                {
                    var supergroupDBs = sqliteDbContext.Supergroups.Where(g => g.Name == supergroup.Ime).ToList();

                    if (supergroupDBs.Any())
                    {
                        SupergroupDB supergroupDB = supergroupDBs.First();
                        supergroupDB.Name = supergroup.Ime;

                        sqliteDbContext.Supergroups.Update(supergroupDB);
                        //supergroups.Add(supergroupDB);
                    }
                    else
                    {
                        int max = 0;

                        if (supergroups.Any())
                        {
                            max = supergroups.Max(s => s.Rb);
                        }
                        SupergroupDB supergroupDB = new SupergroupDB()
                        {
                            Name = supergroup.Ime,
                            Rb = max + 1
                        };
                        supergroups.Add(supergroupDB);
                    }
                });

                await sqliteDbContext.Supergroups.AddRangeAsync(supergroups);
                sqliteDbContext.SaveChanges();
                return supergroups;
            }
            return null;
        }
        public async Task<List<ItemGroupDB>?> ImportGroups(SqlServerDbContext sqliteDbContext)
        {
            string? path = SettingsManager.Instance.GetPathToImportGroups();

            if (!string.IsNullOrEmpty(path))
            {

                List<GroupExcel> excelItems = ImportExcel<GroupExcel>(path, "Grupe");

                List<ItemGroupDB> groups = new List<ItemGroupDB>();

                excelItems.ForEach(async group =>
                {
                    SupergroupDB? supergroupDB = sqliteDbContext.Supergroups.Find(group.Nadgrupa);

                    if (supergroupDB != null)
                    {
                        var itemGroupDBs = sqliteDbContext.ItemGroups.Where(g => g.Name == group.Ime).ToList();

                        if (itemGroupDBs.Any())
                        {
                            ItemGroupDB itemGroupDB = itemGroupDBs.First();
                            itemGroupDB.Name = group.Ime;
                            itemGroupDB.IdSupergroup = group.Nadgrupa;

                            sqliteDbContext.ItemGroups.Update(itemGroupDB);
                            //groups.Add(itemGroupDB);
                        }
                        else
                        {
                            int max = 0;

                            if (groups
                            .Where(i => i.IdSupergroup == group.Nadgrupa).Any())
                            {
                                max = groups
                                .Where(i => i.IdSupergroup == group.Nadgrupa)
                                .Max(s => s.Rb);
                            }

                            ItemGroupDB itemGroupDB = new ItemGroupDB()
                            {
                                Name = group.Ime,
                                IdSupergroup = group.Nadgrupa,
                                Rb = max + 1
                            };
                            groups.Add(itemGroupDB);
                        }
                    }
                });

                await sqliteDbContext.ItemGroups.AddRangeAsync(groups);
                sqliteDbContext.SaveChanges();
                return groups;
            }
            return null;
        }
        public async Task<List<ItemDB>?> ImportItems(SqlServerDbContext sqliteDbContext)
        {
            string? path = SettingsManager.Instance.GetPathToImportItems();

            if (!string.IsNullOrEmpty(path))
            {

                List<ItemExcel> excelItems = ImportExcel<ItemExcel>(path, "Proizvodi");

                List<ItemDB> items = new List<ItemDB>();

                excelItems.ForEach(async item =>
                {
                    ItemGroupDB? itemGroupDB = sqliteDbContext.ItemGroups.Find(item.Grupa);

                    if (itemGroupDB != null)
                    {
                        ItemDB? i = sqliteDbContext.Items.Find(item.Šifra);

                        if (i == null)
                        {

                            int max = 0;

                            if(items
                            .Where(i => i.IdItemGroup == item.Grupa).Any())
                            {
                                max = items
                                .Where(i => i.IdItemGroup == item.Grupa)
                                .Max(i => i.Rb);
                            }

                            i = new ItemDB()
                            {
                                Id = item.Šifra,
                                Rb = max + 1,
                                Label = item.Oznaka,
                                Name = item.Naziv,
                                Jm = item.JM,
                                IdItemGroup = item.Grupa,
                                SellingUnitPrice = item.Cena,
                                InputUnitPrice = 0,
                                TotalQuantity = item.Stanje,
                                AlarmQuantity = item.Alarm,
                                ItemGroupNavigation = itemGroupDB,
                                Procurements = new List<ProcurementDB>()
                            };
                            items.Add(i);
                        }
                        else
                        {
                            i.Label = item.Oznaka;
                            i.Name = item.Naziv;
                            i.Jm = item.JM;
                            i.IdItemGroup = item.Grupa;
                            i.SellingUnitPrice = item.Cena;
                            i.TotalQuantity = item.Stanje;
                            i.AlarmQuantity = item.Alarm;
                            i.ItemGroupNavigation = itemGroupDB;

                            sqliteDbContext.Items.Update(i);
                        }
                    }
                });

                await sqliteDbContext.Items.AddRangeAsync(items);
                sqliteDbContext.SaveChanges();

                return items;
            }
            return null;
        }
        public async Task<List<CashierDB>?> ImportCashiers(SqlServerDbContext sqliteDbContext)
        {
            string? path = SettingsManager.Instance.GetPathToImportCashiers();

            if (!string.IsNullOrEmpty(path))
            {
                if (!File.Exists(path))
                {
                    return null;
                }

                List<CashierExcel> excelCashiers = ImportExcel<CashierExcel>(path, "Kasiri");

                List<CashierDB> cashiers = new List<CashierDB>();

                excelCashiers.ForEach(async cashier =>
                {
                    CashierDB? cashierDB = sqliteDbContext.Cashiers.Find(cashier.Šifra);

                    if (cashierDB != null)
                    {
                        cashierDB.Id = cashier.Šifra;
                        cashierDB.Address = cashier.Adresa;
                        cashierDB.City = cashier.Grad;
                        cashierDB.Email = cashier.Email;
                        cashierDB.Jmbg = cashier.Jmbg;
                        cashierDB.Name = cashier.Ime;
                        cashierDB.ContactNumber = cashier.Telefon;
                        cashierDB.Type = cashier.Pozicija_Radnika;
                        cashierDB.SmartCardNumber = cashier.Broj_Kartice_Za_Prijavu;

                        sqliteDbContext.Cashiers.Update(cashierDB);
                    }
                    else
                    {
                        cashierDB = new CashierDB()
                        {
                            Id = cashier.Šifra,
                            Address = cashier.Adresa,
                            City = cashier.Grad,
                            Email = cashier.Email,
                            Jmbg = cashier.Jmbg,
                            Name = cashier.Ime,
                            ContactNumber = cashier.Telefon,
                            Type = cashier.Pozicija_Radnika,
                            SmartCardNumber = cashier.Broj_Kartice_Za_Prijavu
                        };
                        await sqliteDbContext.Cashiers.AddAsync(cashierDB);
                    }
                    cashiers.Add(cashierDB);
                });

                sqliteDbContext.SaveChanges();
                return cashiers;
            }

            return null;
        }
        public async Task<bool> ExportItems(SqlServerDbContext sqliteDbContext)
        {
            try
            {
                string? path = SettingsManager.Instance.GetPathToExportItems();

                if (!string.IsNullOrEmpty(path))
                {
                    var itemsDB = sqliteDbContext.Items.ToList();

                    List<ItemExcel> items = new List<ItemExcel>();
                    itemsDB.ToList().ForEach(item =>
                    {
                        ItemExcel excelItem = new ItemExcel()
                        {
                            Šifra = item.Id,
                            Cena = item.SellingUnitPrice,
                            Oznaka = item.Label,
                            JM = item.Jm,
                            Naziv = item.Name,
                            Grupa = item.IdItemGroup,
                            Stanje = item.TotalQuantity,
                            Alarm = item.AlarmQuantity
                        };

                        items.Add(excelItem);
                    });

                    return ExportExcel(items, path, "Proizvodi");
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task<bool> ExportCashiers(SqlServerDbContext sqliteDbContext)
        {
            string? path = SettingsManager.Instance.GetPathToExportCashiers();

            if (!string.IsNullOrEmpty(path))
            {
                List<CashierDB> cashiersDB = sqliteDbContext.Cashiers.ToList();

                List<CashierExcel> cashiers = new List<CashierExcel>();
                cashiersDB.ForEach(cashier =>
                {
                    cashiers.Add(new CashierExcel()
                    {
                        Adresa = cashier.Address,
                        Email = cashier.Email,
                        Grad = cashier.City,
                        Ime = cashier.Name,
                        Jmbg = cashier.Jmbg,
                        Telefon = cashier.ContactNumber,
                        Šifra = cashier.Id,
                        Pozicija_Radnika = cashier.Type,
                        Broj_Kartice_Za_Prijavu = cashier.SmartCardNumber
                    });
                });

                return ExportExcel(cashiers, path, "Kasiri");
            }

            return false;
        }
        public async Task<bool> ExportReport(string path, Dictionary<string, List<Report>> reports)
        {
            if (!string.IsNullOrEmpty(path))
            {
                return ExportExcel(reports, path);
            }

            return false;
        }
        public async Task<bool> ExportGroups(SqlServerDbContext sqliteDbContext)
        {
            string? path = SettingsManager.Instance.GetPathToExportGroups();

            if (!string.IsNullOrEmpty(path))
            {
                List<ItemGroupDB> itemGroupDBs = sqliteDbContext.ItemGroups.ToList();

                List<GroupExcel> groups = new List<GroupExcel>();
                itemGroupDBs.ForEach(group =>
                {
                    groups.Add(new GroupExcel()
                    {
                        Ime = group.Name
                    });
                });
                return ExportExcel(groups, path, "Grupe");
            }

            return false;
        }
        #endregion Public methods

        #region Private methods
        private bool ExportExcel<T>(List<T> list, string excelFilePath, string sheetName)
        {
            bool exported = false;

            using (IXLWorkbook workbook = new XLWorkbook())
            {
                workbook.AddWorksheet(sheetName).FirstCell().InsertTable<T>(list, false);

                workbook.SaveAs(excelFilePath);
                exported = true;
            }

            return exported;
        }
        private bool ExportExcel<T>(Dictionary<string, List<T>> list, string excelFilePath)
        {
            bool exported = false;

            using (IXLWorkbook workbook = new XLWorkbook())
            {
                list.ToList().ForEach(l =>
                {
                    workbook.AddWorksheet(l.Key).FirstCell().InsertTable<T>(l.Value, false);
                });

                workbook.SaveAs(excelFilePath);
                exported = true;
            }

            return exported;
        }
        private List<T> ImportExcel<T>(string excelFilePath, string sheetName)
        {
            List<T> list = new List<T>();
            Type typeOfObject = typeof(T);

            if (File.Exists(excelFilePath))
            {
                using (IXLWorkbook workbook = new XLWorkbook(excelFilePath))
                {
                    var worksSheet = workbook.Worksheets.Where(w => w.Name == sheetName).First();
                    var properties = typeOfObject.GetProperties();

                    var columns = worksSheet.FirstRow().Cells().Select((v, i) => new { Value = v.Value, Index = i + 1 });

                    foreach (IXLRow row in worksSheet.RowsUsed().Skip(1))
                    {
                        T obj = (T)Activator.CreateInstance(typeOfObject);

                        foreach (var property in properties)
                        {
                            try
                            {
                                int colIndex = columns.SingleOrDefault(c => c.Value.ToString() == property.Name.ToString()).Index;
                                var val = row.Cell(colIndex).Value;
                                var type = property.PropertyType;

                                if(type.IsEnum)
                                {
                                    CashierTypeEnumeration cashierType;
                                    try
                                    {
                                        int cType = Convert.ToInt32(val);
                                        cashierType = (CashierTypeEnumeration)cType;
                                    }
                                    catch
                                    {
                                        cashierType = (CashierTypeEnumeration)Enum.Parse(typeof(CashierTypeEnumeration), val.ToString());
                                    }
                                    property.SetValue(obj, Convert.ChangeType(cashierType, type));
                                }
                                else
                                {
                                    property.SetValue(obj, Convert.ChangeType(val, type));
                                }
                            }
                            catch (Exception ex)
                            {
                                return null;
                            }
                        }

                        list.Add(obj);
                    }
                }
            }
            else
            {
                return null;
            }

            return list;
        }
        #endregion Private methods
    }
}
