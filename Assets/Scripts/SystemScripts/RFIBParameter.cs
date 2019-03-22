using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public static class RFIBParameter
{
    public static readonly int stageRow = 5;
    public static readonly int stageCol = 9;
    public static readonly int maxHight = 2;

    public static readonly int blockNum = stageRow * stageCol;

    public static readonly int touchRow = 15;
    public static readonly int touchCol = 27;
    public static readonly int notTouchGap = 30;
    public static readonly int maxTouch = 20;

    // 允許甚麼編號被接受
    public static readonly string[] AllowBlockType = {
        "9999",     // 99 floor
        "7101",     // 71 file 1
        "7102",     // 71 file 2
        "7103",     // 71 file 3
        "7601",     // 76 r mask
        "7602",     // 76 g mask
        "7603"      // 76 b mask
	};

    // RFIB_ID對應的instance_ID
    public static int SearchCard(string idStr)
    {
        switch (idStr)
        {
            case "7101": return 0;      // 71 file
            case "7102": return 1;      // 71 file
            case "7103": return 2;      // 71 file
            case "7601": return 10;      // 71 file
            case "7602": return 11;      // 71 file
            case "7603": return 12;      // 71 file

            case "0000": return -1;
        }
        return -1;
    }
}
