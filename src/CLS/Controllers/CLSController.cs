using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CLS.Models;
using CLS;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace CLS.Controllers
{
    [HubName("CLSHub")]
    public class CLSHub : Hub
    {
        public void UpdateClients(string argValue)
        {
            Clients.All.updateContainerPlace(argValue);
        }
    }

    public class CLSController : Controller
    {
        public static CLSModel CLSModel = new CLSModel();
        private Object ContainerMovementLock = new Object();
        private IHubContext _hub;

        public CLSController(IConnectionManager argConnectionManager)
        {
            _hub = argConnectionManager.GetHubContext<CLSHub>();
            
        }

        public IActionResult UpdateClients(List<ContainerPlace> argContainerPlaesToBeUpdated)
        {
            // ToDo: Do the SignalR Stuff here!!
            return RedirectToAction("MainView");
        }

        public IActionResult MainView()
        {
            var claims = ((ClaimsIdentity)User.Identity).Claims.ToList();
            if (!((ClaimsIdentity)User.Identity).Claims.Any(c => c.Type == "CanShowMainView"))
            {
                var msg = "You do not have the priviledges to show the about page! Please login first.";
                return RedirectToAction("Login", "Account", new { argMessage = msg });
            }

            return View(CLSModel);
        }

        [HttpPost]
        public void ChangeContainerPlaceLockingState(string argContainerPlaceId)
        {
            var cp = CLSModel.GetContainerPlaceById(argContainerPlaceId);
            if (cp == null)
                return;
            // handle locking/unlocking of Crane (we allow just one crane at a time!)
            if (cp.ContainerPlaceType == "3")
            {
                var lockedCP = CLSModel.GetCranePlaces().Single(c => c.IsLocked == true);
                var unlockedCP = CLSModel.GetCranePlaces().Single(c => c.IsLocked == false);
                lockedCP.IsLocked = false;
                unlockedCP.IsLocked = true;
                UpdateClients(new List<ContainerPlace>() { lockedCP, unlockedCP });
                return;

            }

            cp.IsLocked = !cp.IsLocked;
            UpdateClients(new List<ContainerPlace>() { cp });
            return;

        }
    }
}
