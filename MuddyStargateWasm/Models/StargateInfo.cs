namespace MuddyStargateWasm.Models
{
    public enum StargateType
    {
        MilkyWay, // SG1
        Pegasus, 
        Destiny, // SG:Universe
        Dawn, // Resonite
        Aperture, // Portal
        Omni, // No clue
        Unknown
    }

    public class StargateInfo
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

        public string FullCode => $"{gate_address}{gate_code}";
        public string CleanCode => gate_code.Replace("@", "");

        public StargateType Type => gate_code switch
        {
            var code when code == "M@" => StargateType.MilkyWay,
            var code when code == "R@" => StargateType.Dawn,
            var code when code == "U@" => StargateType.Destiny,
            var code when code == "TE" => StargateType.Omni,
            var code when code == "P@" => StargateType.Pegasus,
            var code when code == "??" => StargateType.Aperture, // TODO: Find out what the code for Aperture is, this is just a placeholder
            _ => StargateType.Unknown
        };

        public bool UseCustomFont { get; set; } = true;

        // https://www.thescifiworld.net/fonts.htm
        public string AddressFontClass => Type switch
        {
            var code when !UseCustomFont => "",
            StargateType.Destiny => "stargate-universe-font",
            StargateType.MilkyWay => "stargate-milkyway-goauld-font",
            StargateType.Pegasus => "stargate-pegasus-font",
            
            _ => ""
        };
    }


}
