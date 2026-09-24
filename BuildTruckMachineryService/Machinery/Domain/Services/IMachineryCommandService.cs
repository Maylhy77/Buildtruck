using BuildTruckMachineryService.Machinery.Domain.Model.Commands;

namespace BuildTruckMachineryService.Machinery.Domain.Services;

public interface IMachineryCommandService
{
    Task<Model.Aggregates.Machinery> Handle(CreateMachineryCommand command, IFormFile? imageFile = null);
    Task<Model.Aggregates.Machinery> Handle(UpdateMachineryCommand command, IFormFile? imageFile = null);
    Task Handle(DeleteMachineryCommand command);
   
}