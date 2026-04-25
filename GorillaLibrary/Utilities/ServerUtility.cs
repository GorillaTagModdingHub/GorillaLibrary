using Photon.Pun;

namespace GorillaLibrary.Utilities;

public static class ServerUtility
{
    public static string GetRegionCode() => (PhotonNetwork.CloudRegion ?? NetworkSystem.Instance.CurrentRegion).Replace("/*", "");

    public static string GetRegionName() => GetRegionNameFromCode(GetRegionCode());

    public static string GetRegionNameFromCode(string regionCode) => regionCode switch
    {
        "us" => "United States (East)", // Washington DC
        "usw" => "United States (West)", // San Jose
        "eu" => "Europe", // Amsterdam
        "asia" => "Asia", // Singapore
        "au" => "Australia", // Sydney
        "cae" => "Canada", // Montreal
        "hk" => "Hong Kong", // Hong Kong
        "in" => "India", // Chennai
        "jp" => "Japan", // Tokyo
        "za" => "South Africa", // Johannesburg
        "sa" => "South America", // Sao Paulo
        "kr" => "South Korea", // Seoul
        "tr" => "Turkey", // Istanbul
        "uae" => "United Arab Emirates", // Dubai
        "ussc" => "United States (South Central)", // Dallas
        _ => regionCode
    };
}
