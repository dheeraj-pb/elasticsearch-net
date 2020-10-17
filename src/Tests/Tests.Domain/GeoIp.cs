using Nest6;

namespace Tests.Domain
{
	public class GeoIp
	{
		public string CityName { get; set; }
		public string ContinentName { get; set; }

		public string CountryIsoCode { get; set; }

		public GeoLocation Location { get; set; }

		public string RegionName { get; set; }
	}
}
