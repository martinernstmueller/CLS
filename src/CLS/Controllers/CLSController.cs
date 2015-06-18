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
        private static readonly ICollection<string> Users = new List<string>();
        public static int seconds = 0;

        public CLSHub()
        {
            
        }

        public void Send(string Message)
        {
            Clients.All.showMessage(Message);
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

        public IActionResult MainView()
        {
            var claims = ((ClaimsIdentity)User.Identity).Claims.ToList();
            if (!((ClaimsIdentity)User.Identity).Claims.Any(c => c.Type == "CanShowMainView"))
            {
                var msg = "You do not have the priviledges to show the Main View! Please login first.";
                return RedirectToAction("Login", "Account", new { argMessage = msg });
            }
            ViewBag.CanShowContextMenu = ((ClaimsIdentity)User.Identity).Claims.Any(c => c.Type == "CanShowContextMenu");
            ViewBag.CanLockContainerPlaces = ((ClaimsIdentity)User.Identity).Claims.Any(c => c.Type == "CanLockContainerPlaces");

            return View(CLSModel);
        }

        [HttpPost]
        public void ChangeContainerPlaceLockingState(string argContainerPlaceId)
        {
            //_hub.Clients.All.showMessage("Server noticfied a clock on Container with Id " + argContainerPlaceId);
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
                // ToDo: Send multiple cps to the view
                var cpupdates = new List<ContainerPlace>() { lockedCP, unlockedCP };
                _hub.Clients.All.UpdateContainerPlaces(cpupdates);
                return;
            }

            cp.IsLocked = !cp.IsLocked;
            _hub.Clients.All.UpdateContainerPlace(cp);
            return;

        }

        [HttpPost]
        public void PickUpContainerFromContainerPlace(string argContainerPlaceId)
        {
            var cp = CLSModel.GetContainerPlaceById(argContainerPlaceId);
            if (cp == null)
            {
                // ToDo: the error handling here!!
                _hub.Clients.All.showMessage("Could not load container to crane. Container with Id " + cp.Id + " could not be found!");
                return;
            }

            var crane = CLSModel.CranePlaces.Single(cr => cr.IsLocked == false);

            if (crane.Container != null)
            {
                // ToDo: the error handling here!!
                _hub.Clients.All.showMessage("Could not load container to crane with id " + crane.Id + " -> Crane is already occupied!");
                return;
            }
            if (cp.ContainerPlaceType == "3")
            {
                // error! We could not pick up a container from a crane!
                // ToDo: the error handling here!!
                _hub.Clients.All.showMessage("Could not pick up a container from  " + crane.Id + " -> Crane is already occupied!");
                return;
            }
            if (cp.ContainerPlaceType == "1") // its a transfer car
            {
                // assign the container to the Crane
                var tc = (TransferCarPlace)cp;
                crane.Container = tc.Container;
                tc.Container = null;
                var cpupdates = new List<ContainerPlace>() { crane, tc};
                _hub.Clients.All.UpdateContainerPlaces(cpupdates);
                return;

            }

            return;

        }

        [HttpPost]
        public void DropDownContainerFromContainerPlace(string argContainerPlaceId)
        {
            var crane = CLSModel.CranePlaces.Single(cr => cr.IsLocked == false);
            if (crane.Container == null)
            {
                // ToDo: the error handling here!!
                _hub.Clients.All.showMessage("Could not drop container on Container place with id  " + argContainerPlaceId + 
                    " -> There is no Container on Crane with id " + crane.Id + "!");
                return;
            }
            var cp = CLSModel.GetContainerPlaceById(argContainerPlaceId);
            if (cp == null)
            {
                // ToDo: the error handling here!!
                _hub.Clients.All.showMessage("Could not drop container to container place with Id " + argContainerPlaceId + "! Container place not could not be found!");
                return;
            }

            if (cp.ContainerPlaceType == "3")
            {
                // ToDo: the error handling here!!
                _hub.Clients.All.showMessage("Can not drop a container onto a crane!");
                return;
            }
            if (cp.ContainerPlaceType == "1") // its a transfer car
            {
                // assign the container to the transfer car
                var tc = (TransferCarPlace)cp;
                tc.Container = crane.Container;
                crane.Container = null;
                var cpupdates = new List<ContainerPlace>() { crane, tc };
                _hub.Clients.All.UpdateContainerPlaces(cpupdates);
                return;
            }
            return;
        }
    }
}
