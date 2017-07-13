using System.Threading.Tasks;

namespace Wally.Day_Dream.Interfaces
{
    internal interface IPictureDataProvider
    {
        bool CanSupplyWhenFail { get; }
        Task<PictureData> GetData();
    }
}