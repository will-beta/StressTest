using System.ServiceModel;
using System.ServiceModel.Web;

namespace Contract
{
    [ServiceContract]
    public interface IStressService
    {
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        StressResult Stress();
    }
}
