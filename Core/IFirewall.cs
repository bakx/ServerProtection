using NetFwTypeLib;
using SP.Core.Models;

namespace SP.Core
{
    public interface IFirewall
    {
        /// <summary>
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="block"></param>
        void Block(NET_FW_IP_PROTOCOL_ protocol, Blocks block);

        /// <summary>
        /// </summary>
        void Unblock();
    }
}