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

    public enum GateStatus
    {
        OPEN,
        IDLE,
        INCOMING7,
        INCOMING8,
        INCOMING9,
        UNKNOWN
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


        //https://github.com/Ancients-of-Resonite/stargate-network/blob/master/apps/sgn-socket/src/types/stargate.ts#L13C26-L13C35
        public GateStatus Status => gate_status switch
        {
            var code when code == "IDLE" => GateStatus.IDLE,
            var code when code == "OPEN" => GateStatus.OPEN,
            var code when code == "INCOMING7" => GateStatus.INCOMING7,
            var code when code == "INCOMING8" => GateStatus.INCOMING8,
            var code when code == "INCOMING9" => GateStatus.INCOMING9,
            _ => GateStatus.UNKNOWN
        };

        public bool UseCustomFont { get; set; } = true;

        // https://www.thescifiworld.net/fonts.htm

        // TODO: load these: https://github.com/Ancients-of-Resonite/stargate-network/tree/master/apps/sgn-dash/src/assets
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
