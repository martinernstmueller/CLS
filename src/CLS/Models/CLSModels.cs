using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CLS.Models
{
    public class CLSModel
    {
        public static Container ContainerInAir { get; set; }
        public static List<Container> Containers = new List<Container>();
        public IEnumerable<Container> GetContainers()
        {
            return Containers;
        }

        public static ContainerPlace GetContainerPlaceById(string argContainerId)
        {
            ContainerPlace retval = TransferCarPlaces.Find(c => c.Id == argContainerId);
            if (retval != null)
                return retval;

            retval = StockPlaces.Find(c => c.Id == argContainerId);
            if (retval != null)
                return retval;

            retval = CranePlaces.Find(c => c.Id == argContainerId);
            if (retval != null)
                return retval;

            retval = BargePlaces.Find(c => c.Id == argContainerId);
            if (retval != null)
                return retval;

            return null;
        }

        public static List<TransferCarPlace> TransferCarPlaces = new List<TransferCarPlace>();
        public IEnumerable<TransferCarPlace> GetTransferCarPlaces()
        {
            return TransferCarPlaces;
        }

        public static List<CranePlace> CranePlaces = new List<CranePlace>();
        public IEnumerable<CranePlace> GetCranePlaces()
        {
            return CranePlaces;
        }

        public static List<StockPlace> StockPlaces = new List<StockPlace>();
        public IEnumerable<StockPlace> GetStockPlaces()
        {
            return StockPlaces;
        }

        public static List<ContainerPlace> BargePlaces = new List<ContainerPlace>();
        public IEnumerable<ContainerPlace> GetBargePlaces()
        {
            return BargePlaces;
        }

    }

    public class Container
    {
        public String Id { get; set; }
        public Double Weight { get; set; }
    }

    public class ContainerPlace
    {
        public String ContainerPlaceType;
        public String Id { get; set; }
        public Position Position { get; set; }
        public bool IsLocked { get; set; }
    }

    public class TransferCarPlace : ContainerPlace
    {
        public Container Container { get; set; }
        public TransferCarPlace()
        {
            ContainerPlaceType = "1";
        }
    }

    public class StockPlace : ContainerPlace
    {
        public Container UpperContainer { get; set; }
        public Container LowerContainer { get; set; }

        public StockPlace()
        {
            ContainerPlaceType = "2";
        }
    }


    public class CranePlace : ContainerPlace
    {
        public Container Container { get; set; }
        public Container LowerContainer { get; set; }

        public CranePlace()
        {
            ContainerPlaceType = "3";
        }
    }

    public class Position
    {
        public int xPos { get; set; }
        public int yPos { get; set; }
    }
}