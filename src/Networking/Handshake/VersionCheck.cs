using System.Linq;
using HarmonyLib;
using VentLib.Logging;
using VentLib.Networking.RPC;
using VentLib.Networking.RPC.Attributes;
using VentLib.Utilities;
using VentLib.Networking.Handshake.Patches;
using VentLib.Version;

namespace VentLib.Networking.Handshake;

public class VersionCheck
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(VersionCheck));
    // This is the sender version of this Rpc. In order to fully utilize it you must make your own handler.
    [VentRPC(VentCall.VersionCheck, RpcActors.Host, RpcActors.NonHosts)]
    public static void RequestVersion()
    {
        SendVersion(VersionControl.Instance.Version ?? new NoVersion());
    }

    [VentRPC(VentCall.VersionCheck, RpcActors.NonHosts, RpcActors.LastSender)]
    public static void SendVersion(Version.Version version)
    {
        PlayerControl? lastSender = Vents.GetLastSender((uint)VentCall.VersionCheck);
        if (lastSender == null) return;
        log.Info($"Received Version: \"{version.ToSimpleName()}\" from Player {lastSender.Data?.PlayerName}", "VentLib");
        VersionControl vc = VersionControl.Instance;

        if (lastSender != null)
            PlayerJoinPatch.WaitSet.Remove(lastSender.GetClientId());
        
        HandshakeResult action = vc.HandshakeFilter!.Invoke(version);
        vc.VersionHandles
            .Where(h => h.Item1.HasFlag(action is HandshakeResult.PassDoNothing ? ReceiveExecutionFlag.OnSuccessfulHandshake : ReceiveExecutionFlag.OnFailedHandshake))
            .Do(h => h.Item2.Invoke(version, lastSender!));
        
        HandleAction(action, lastSender);
    }

    private static void HandleAction(HandshakeResult action, PlayerControl? player)
    {
        if (player == null) return;
        switch (action)
        {
            case HandshakeResult.DisableRPC:
                Vents.BlockClient(Vents.RootAssemby, player.GetClientId());
                break;
            case HandshakeResult.Kick:
                AmongUsClient.Instance.KickPlayer(player.GetClientId(), false);
                break;
            case HandshakeResult.Ban:
                AmongUsClient.Instance.KickPlayer(player.GetClientId(), true);
                break;
            case HandshakeResult.PassDoNothing:
                VersionControl.Instance.PassedClients.Add(player.GetClientId());
                break;
            case HandshakeResult.FailDoNothing:
            default:
                break;
        }
        
    }
}