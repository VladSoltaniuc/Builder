// Presentation layer — SignalR hub for live order updates
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ProductApi.Hubs;

[Authorize]
public class OrderHub : Hub { }
