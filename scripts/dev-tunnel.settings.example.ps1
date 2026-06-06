@{
    # Use "token" for a stable named Cloudflare Tunnel.
    # Use "quick" only for ad-hoc TryCloudflare development tunnels.
    FrontendTunnelMode = "token"

    # Copy the eyJ... token from Cloudflare Zero Trust > Networks > Tunnels > <your tunnel> > Add a replica.
    FrontendTunnelToken = "paste-your-cloudflared-token-here"

    # Public hostname already routed to that tunnel in Cloudflare.
    FrontendPublicUrl = "https://frontend-dev.your-domain.example"
}
