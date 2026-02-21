using UnityEngine;

namespace Injector
{
    [Inject(typeof(IEchoService))]
    public sealed partial class EchoServiceConsumer : MonoBehaviour
    {
        public void OnEnable() => this.EchoService.Echo("Test");
    }
}