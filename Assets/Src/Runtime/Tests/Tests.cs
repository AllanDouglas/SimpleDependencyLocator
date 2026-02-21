using UnityEngine;

namespace Injector
{

    public interface IEchoService : IService
    {
        public void Echo(string echo);
    }
    
    public sealed class EchoService : IEchoService
    {
        public void Echo(string echo)
        {
            Debug.Log(echo);
        }
    }
}