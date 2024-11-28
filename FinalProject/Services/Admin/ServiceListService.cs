using FinalProject.Data;
using FinalProject.ViewModels.Admin;

namespace FinalProject.Services.Admin
{
    public class ServiceListService
    {
        private readonly QlptContext db;

        public ServiceListService(QlptContext db)
        {
            this.db = db;
        }

        public int GetServiceListCount()
        {
            return db.Services.Count();
        }

        public List<ServiceListVM> GetServices(int pageNumber, int pageSize)
        {
            return db.Services
                .OrderBy(p => p.ServiceName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ServiceListVM
                {
                    ServiceId = p.ServiceId,
                    ServiceName = p.ServiceName,
                    ServiceDescription = p.ServiceDescription,
                    ServicePrice = (decimal)p.ServicePrice,
                    ServiceTime = (int)p.ServiceTime
                })
                .ToList();
        }

        public List<ServiceListVM> SearchService(string searchQuery)
        {
            try
            {
                return db.Services
                    .Where(u => u.ServiceName.Contains(searchQuery))
                    .Select(p => new ServiceListVM
                    {
                        ServiceId = p.ServiceId,
                        ServiceName = p.ServiceName,
                        ServiceDescription = p.ServiceDescription,
                        ServicePrice = (decimal)p.ServicePrice,
                        ServiceTime = (int)p.ServiceTime
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public bool AddService(ServiceListVM serviceListVM)
        {
            try
            {
                var service = new Service
                {
                    ServiceName = serviceListVM.ServiceName,
                    ServiceDescription = serviceListVM.ServiceDescription,
                    ServicePrice = serviceListVM.ServicePrice,
                    ServiceTime = serviceListVM.ServiceTime
                };

                db.Services.Add(service);

                db.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public ServiceListVM GetServiceById(int serviceId)
        {
            try
            {
                var service = db.Services.FirstOrDefault(s => s.ServiceId == serviceId);

                if (service != null)
                {
                    ServiceListVM serviceListVM = new ServiceListVM
                    {
                        ServiceId = service.ServiceId,
                        ServiceDescription = service.ServiceDescription,
                        ServiceName = service.ServiceName,
                        ServicePrice = (decimal)service.ServicePrice,
                        ServiceTime = (int)service.ServiceTime
                    };

                    return serviceListVM;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public bool UpdateService(ServiceListVM serviceListVM)
        {
            try
            {
                var existingService = db.Services.FirstOrDefault(s => s.ServiceId == serviceListVM.ServiceId);

                if (existingService != null)
                {
                    existingService.ServiceName = serviceListVM.ServiceName;
                    existingService.ServiceDescription = serviceListVM.ServiceDescription;
                    existingService.ServiceTime = serviceListVM.ServiceTime;
                    existingService.ServicePrice = serviceListVM.ServicePrice;


                    db.SaveChanges();

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool CanDeleteService(int serviceId)
        {
            return !db.Bills.Any(b => b.ServiceId == serviceId);
        }

        public bool DeleteService(int serviceId)
        {
            var service = db.Services.FirstOrDefault(s => s.ServiceId == serviceId);
            if (service == null)
            {
                return false;
            }

            db.Services.Remove(service);
            db.SaveChanges();
            return true;
        }
    }
}
