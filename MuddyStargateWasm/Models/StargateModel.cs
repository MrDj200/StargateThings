namespace MuddyStargateWasm.Models
{
    public class StargateModel
    {
        public string gate_address { get; set; } = null!;
        public string gate_code { get; set; } = null!;
        public string gate_status { get; set; } = null!;
        public string owner_name { get; set; } = null!;
        public string session_name { get; set; } = null!;
        public string session_url { get; set; } = null!;
        public int active_users { get; set; }
        public int max_users { get; set; }
        public bool public_gate { get; set; }
        public bool is_headless { get; set; }
        public bool iris_state { get; set; }
    }


}
