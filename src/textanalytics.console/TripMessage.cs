using System;
using System.Collections.Generic;
using System.Text;

namespace textanalytics.console
{
	public struct TripMessage
	{
		public int trip_type { get; set; }
		public string trip_year { get; set; }
		public string trip_month { get; set; }
		public string taxi_type { get; set; }
		public int vendor_id { get; set; }
		public DateTime pickup_datetime { get; set; }
		public DateTime dropoff_datetime { get; set; }
		public int passenger_count { get; set; }
		public double trip_distance { get; set; }
		public int rate_code_id { get; set; }
		public string store_and_fwd_flag { get; set; }
		public int pickup_location_id { get; set; }
		public int dropoff_location_id { get; set; }
		public string pickup_longitude { get; set; }
		public string pickup_latitude { get; set; }
		public string dropoff_longitude { get; set; }
		public string dropoff_latitude { get; set; }
		public int payment_type { get; set; }
		public double fare_amount { get; set; }
		public double extra { get; set; }
		public double mta_tax { get; set; }
		public double tip_amount { get; set; }
		public double tolls_amount { get; set; }
		public double improvement_surcharge { get; set; }
		public double ehail_fee { get; set; }

		public double total_amount
		{
			get
			{
				return this.fare_amount + this.extra + this.mta_tax + this.tip_amount + this.tolls_amount + this.improvement_surcharge + this.ehail_fee;
			}
		}

		public string customer_comments { get; set; }
	}
}
