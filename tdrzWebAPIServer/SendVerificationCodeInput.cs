using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tdrzWebAPIServer
{
    public class SendVerificationCodeInput
    {
        public string ReservationNo { get; set; }
        public string HotelId { get; set; }
        public string Mobile { get; set; }

        public override string ToString()
        {
            return $"ReservationNo:{ReservationNo}, HotelId:{HotelId}, Mobile:{Mobile}";
        }
    }

    public class AuthenticationsInput
    {
        public string ReservationNo { get; set; }
        public string HotelId { get; set; }
        public string Mobile { get; set; }
        public string VerificationCode { get; set; }
    }
}
