namespace Awesome{
    public class Repo{
        public async Task<GridResult<CarSearchDto>> SearchVehicles(Token token, bool onlyHasExternalId, int? saleId, int? pageSize, int? pageNumber, GridPagingSortFilter searchParams, string sort = "", string filter = "")
        {
            var context = DbContext;
            var isAdminUserType = token.IsAdminUserType();
            List<CarDto> fixedCarDtoList = null;

            var matchValue = filter.Length > 0 && searchParams != null && searchParams.Filter.Logic == "or"
                ? searchParams.Filter.Filters[0].FieldValue
                : "";

            var VehiclesQuery = from Vehicle in context.Vehicles
                                    from Make in context.Make.Where(x => x.MakeId == Vehicle.MakeId).DefaultIfEmpty()
                                    from CarType in context.CarType.Where(x => x.CarTypeId == Vehicle.CarTypeId).DefaultIfEmpty()
                                    let Manufacturer = Vehicle.Manufacturer
                                    let manu = Manufacturer
                                    let sale = Manufacturer.Sale
                                    let PreviousOwner = Manufacturer.PreviousOwner
                                    let Vehicles = Vehicle.LegalDocsVehicles.Where(phe => phe.IsActive && phe.Car.IsRequired && phe.Car.IsActive)
                                    let ManufacturerName = manu.AdminManufacturerName ?? ((manu.SoldBy ?? "") + ((manu.SoldBy != null && manu.PropertyOf != null) ? "/" : "") + (manu.PropertyOf ?? ""))
                                    let numberOfOpenClaims = Vehicles.Count(x => new List<int>()
                                        {
                                            (int)Constants.LegalDocsStatus.NotReceived,
                                            (int)Constants.LegalDocsStatus.OutstandingRequirements,
                                            (int)Constants.LegalDocsStatus.NeedsCorrection
                                        }.Contains(x.LegalDocsStatusId))
                                    where Vehicle.IsActive && Manufacturer.IsActive && sale.IsActive
                                    select new CarSearchDto()
                                    {
                                        SaleId = sale.SaleId,
                                        VehicleId = Vehicle.VehicleId,
                                        BeginSaleDate = sale.BeginSaleDate,
                                        OwnerName = Vehicle.VehicleOwners.Select(he => he.Owner.OwnerName).FirstOrDefault(),
                                        match = Vehicle.VehicleOwners.Any(heo => heo.Owner.OwnerName.ToLower().Contains(matchValue.ToLower())),
                                        PreviousOwnerName = PreviousOwner.PreviousOwnerName,
                                        PreviousOwnerId = PreviousOwner.PreviousOwnerId,
                                        PreviousSellerName = Vehicle.PreviousSellerName,
                                        SaleName = sale.SaleName,
                                        SalvageTitleDate = Vehicle.SalvageTitleDate,
                                        ExternalId = Vehicle.ExternalId,
                                        AssignedGarage = Vehicle.AssignedGarage,
                                        ManufacturerId = Manufacturer.ManufacturerId,
                                        ManufacturerName = ManufacturerName,
                                        SoldBy = Manufacturer.SoldBy,
                                        PropertyOf = Manufacturer.PropertyOf,
                                        TitleAgencyName = Vehicle.TitleAgencyName,
                                        BMVTransmittalNumber = Vehicle.BMVTransmittalNumber,
                                        SaleStatus = Vehicle.VehicleSaleStatus,
                                        SaleContractStorageFileNamePath = Vehicle.VehicleContracts.FirstOrDefault(x => x.IsActive).StorageFileNamePath,
                                        SaleAmount = Vehicle.SaleAmount,
                                        StatementUrl = Vehicle.VehiclePreviousOwnerStatements.FirstOrDefault(x => x.IsActive).StorageFileNamePath,
                                        StatementUpdate = Vehicle.VehiclePreviousOwnerStatements.FirstOrDefault(x => x.IsActive).StatementPostedDate,
                                        MileageProofUrl = Vehicle.VehicleMileages.FirstOrDefault(x => x.IsActive).StorageUrl,
                                        MileageStorageFileNamePath = Vehicle.VehicleMileages.FirstOrDefault(x => x.IsActive).StorageFileNamePath,
                                        WebsiteViews = Vehicle.WebsiteActivities.Count(a => a.SaleId == sale.SaleId && a.ExternalId == Vehicle.ExternalId),
                                        VehicleStatus = !isAdminUserType && Vehicle.IsDisplayVehicleStatusOverride && Vehicle.VehicleStatu.IsDisplayExternal && Vehicle.VehicleStatu.IsActive ? Vehicle.VehicleStatu.VehicleStatusExternal : Vehicle.VehicleStatu.VehicleStatusInternal,
                                        VehicleSalvageStatusCode = Vehicle.VehicleStatu.VehicleSalvageStatusCode,
                                        Claimstatus = numberOfOpenClaims == 0,
                                        ImpoundPaid = Vehicle.FeePaidDate.HasValue,
                                        MakeId = Make != null ? Make.MakeId : 0,
                                        TypeId = CarType != null ? CarType.TypeId : "",
                                        ConvertableMoonroofStatusId = Vehicle.ConvertableMoonroofStatusId,
                                        Convertable = Vehicle.Convertable
                                    };

            if (saleId.HasValue)
            {
                VehiclesQuery = VehiclesQuery.Where(h => h.SaleId == saleId);
            }
            if (token.PreviousOwnerId.HasValue)
            {
                VehiclesQuery = VehiclesQuery.Where(q => q.PreviousOwnerId == token.PreviousOwnerId.Value);
            }

            if (onlyHasExternalId)
            {
                string temp = "ExternalId != null";
                VehiclesQuery = VehiclesQuery.Where(temp);
            }

            if (filter.Length > 0)
            {
                if (searchParams != null && searchParams.Filter.Logic == "or")
                {
                    filter += " or match";
                }
                VehiclesQuery = VehiclesQuery.Where(filter);
            }

            if (sort.Length == 0)
            {
                // Default sort order
                VehiclesQuery = VehiclesQuery.OrderByDescending(i => i.ExternalId.HasValue).ThenBy(i => i.ExternalId).ThenBy(i => i.PreviousOwnerName).ThenBy(i => i.ManufacturerName);
            }
            else
            {
                VehiclesQuery = VehiclesQuery.OrderBy(sort);
            }


            IEnumerable<CarSearchDto> Vehicles;
            if (pageSize.HasValue && pageNumber.HasValue)
            {
                Vehicles = await VehiclesQuery.Skip(pageNumber.Value > 0 ? (pageNumber.Value - 1) * (pageSize.Value) : 0)
                                      .Take(pageSize.Value)
                                      .ToListAsync();
            }
            else
            {
                Vehicles = await VehiclesQuery.ToListAsync();
            }

            int index = 0;
            foreach (var item in Vehicles.ToList())
            {
                if (index == 0)
                {
                    var dbFixedCarList = await context.Cars.Where(x => x.SaleId == item.SaleId && x.IsActive && (x.IsFixedLegalDocsRequirements || x.SystemCarTypeId == (int)Constants.SystemCarType.PaymentWithdrawal)).ToListAsync();
                    fixedCarDtoList = dbFixedCarList.Select(x => Mapper.Map(x, new CarDto())).ToList();
                }
                if ((fixedCarDtoList != null) && (fixedCarDtoList.Count > 0) && (item.Claimstatus))
                {
                    var flag = ComputeVehicleSearchClaimstatus(fixedCarDtoList, item.VehicleId,
                        item.MakeAdminCodeId, item.TypeId, item.BeginSaleDate, item.SalvageTitleDate,
                        item.ConvertableMoonroofStatusId, item.SaleStatus);
                    if (flag)
                    {
                        item.Claimstatus = false;
                    }
                }
                index++;
            }

            // refresh data
            Vehicles = Vehicles.Select(car =>
            {
                car.MileageCatalogUrl = car.BeginSaleDate.HasValue
                    ? $"http://www.carlease.com/catalogs/{car.BeginSaleDate.Value.ToString("yyyy/MMdd")}/{car.ExternalId}_catalog_template.doc"
                    : null;
                return car;
            });

            var result = new GridResult<CarSearchDto>
            {
                Total = await VehiclesQuery.CountAsync(),
                TableData = Vehicles
            };

            return result;
        }
    }
}
