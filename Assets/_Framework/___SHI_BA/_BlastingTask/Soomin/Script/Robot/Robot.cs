using System;
using static UnityEngine.Rendering.DebugUI;

public class Robot
{
    public RobotData Data { get; set; }
}

public class RobotData
{
    public ActPosKukaFK FK { get; set; }
    public ActPosExtAxis ExtAxis { get; set; }
}

public class ActPosKukaFK
{
    private double th1, th2, th3, th4, th5, th6;

    public double TH1
    {
        get => th1;
        set => th1 = value + 270;  // 할당 시 계산 적용
    }

    public double TH2
    {
        get => th2;
        set => th2 = value + 90;
    }

    public double TH3
    {
        get => th3;
        set => th3 = (value * -1) + 90;
    }

    public double TH4
    {
        get => th4;
        set => th4 = (value * -1);
    }

    public double TH5
    {
        get => th5;
        set => th5 = (value * -1);
    }

    public double TH6
    {
        get => th6;
        set => th6 = (value * -1);
    }
 
   
}

public class ActPosExtAxis
{
    private double e1, e2, e3, e4, e5, e6;

    public double E1
    {
        get => e1;
        set => e1 = value * -1 / 1000;
    }

    public double E2
    {
        get => e2;
        set => e2 = value / 100;
    }

    public double E3
    {
        get => e3;
        set => e3 = NormalizeByRange(value / 100, 19, 89);
        
    }

    public double E4
    {
        get => e4;
        set => e4 = value * -1;
    }

    public double E5
    {
        get => e5;
        set => e5 = value * - 1;
    }

    public double E6
    {
        get => e6;
        set => e6 = value ;
    }

    private double NormalizeByRange(double value, double minValue, double maxValue)
    {
        return (value - minValue) / (maxValue - minValue) * 62.0;
    }

}