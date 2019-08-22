using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LabToRGB
{
    public static Vector3 Convert(Vector3 labColor)
    {
        /*
         * from http://www.easyrgb.com/en/math.php#text8, convert LAB -> XYZ, XYZ -> RGB)
         */

        // Convert LAB to XYZ
        var var_Y = (labColor.x + 16) / 116f;
        var var_X = labColor.y / 500f + var_Y;
        var var_Z = var_Y - labColor.z / 200f;

        if (Mathf.Pow(var_Y, 3) > 0.008856f) var_Y = Mathf.Pow(var_Y, 3);
        else var_Y = (var_Y - 16 / 116f) / 7.787f;

        if (Mathf.Pow(var_X, 3) > 0.008856f) var_X = Mathf.Pow(var_X, 3);
        else var_X = (var_X - 16 / 116f) / 7.787f;

        if (Mathf.Pow(var_Z, 3) > 0.008856) var_Z = Mathf.Pow(var_Z, 3);
        else var_Z = (var_Z - 16 / 116f) / 7.787f;

        Vector3 xyzColor = new Vector3(var_X, var_Y, var_Z);

        // Convert XYZ to RGB
        var_X = xyzColor.x;// / 100f;
        var_Y = xyzColor.y;// / 100f;
        var_Z = xyzColor.z;// / 100f;

        var var_R = var_X * 3.2406f + var_Y * -1.5372f + var_Z * -0.4986f;
        var var_G = var_X * -0.9689f + var_Y * 1.8758f + var_Z * 0.0415f;
        var var_B = var_X * 0.0557f + var_Y * -0.2040f + var_Z * 1.0570f;

        if (var_R > 0.0031308f) var_R = 1.055f * (Mathf.Pow(var_R, (1 / 2.4f))) - 0.055f;
        else var_R = 12.92f * var_R;

        if (var_G > 0.0031308f) var_G = 1.055f * (Mathf.Pow(var_G, (1 / 2.4f))) - 0.055f;
        else var_G = 12.92f * var_G;

        if (var_B > 0.0031308f) var_B = 1.055f * (Mathf.Pow(var_B, (1 / 2.4f))) - 0.055f;
        else var_B = 12.92f * var_B;

        return new Vector3(var_R, var_G, var_B);
    }
}
