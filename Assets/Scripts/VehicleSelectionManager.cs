using UnityEngine;
using System;

public enum VehicleType
{
    Excavator = 0,  // Экскаватор
    Tractor = 1,    // Трактор
    Kamaz = 2       // КамАЗ (Самосвал)
}

[System.Serializable]
public class VehicleInfo
{
    public VehicleType type;
    public string displayName;
    public string description;
    public string controlsHint;
    public GameObject prefab;
    public Sprite icon;
}

public static class VehicleSelectionManager
{
    private const string PREF_KEY = "SelectedVehicleType";

    public static VehicleType SelectedVehicle
    {
        get => (VehicleType)PlayerPrefs.GetInt(PREF_KEY, (int)VehicleType.Kamaz);
        set
        {
            PlayerPrefs.SetInt(PREF_KEY, (int)value);
            PlayerPrefs.Save();
            OnVehicleChanged?.Invoke(value);
        }
    }

    public static event Action<VehicleType> OnVehicleChanged;

    public static readonly VehicleInfo[] AvailableVehicles = new VehicleInfo[]
    {
        new VehicleInfo
        {
            type = VehicleType.Kamaz,
            displayName = "КамАЗ (Самосвал)",
            description = "Тяжёлый грузовик для перевозки и разгрузки сыпучих материалов. Высокая скорость и манёвренность.",
            controlsHint = "W/S — Газ/Тормоз\nA/D — Руление\nQ/E — Подъём/Опускание кузова\nF — Фары, H — Сигнал"
        },
        new VehicleInfo
        {
            type = VehicleType.Excavator,
            displayName = "Экскаватор",
            description = "Гусеничный экскаватор для земляных работ. Поворот башни на 360°, управление стрелой и ковшом.",
            controlsHint = "W/S — Движение гусениц\nA/D — Поворот\nСтрелки/Мышь — Стрела и ковш\nQ/E — Поворот башни"
        },
        new VehicleInfo
        {
            type = VehicleType.Tractor,
            displayName = "Колёсный Трактор",
            description = "Универсальный тягач с фронтальным отвалом/ковшом и коробкой передач.",
            controlsHint = "W/S — Газ/Тормоз\nA/D — Руление колес\n1-5 — Передачи\nQ/E — Наклон ковша"
        }
    };

    public static VehicleInfo GetInfo(VehicleType type)
    {
        foreach (var info in AvailableVehicles)
        {
            if (info.type == type) return info;
        }
        return AvailableVehicles[0];
    }

    public static void SelectNext()
    {
        int count = AvailableVehicles.Length;
        int nextIndex = ((int)SelectedVehicle + 1) % count;
        SelectedVehicle = (VehicleType)nextIndex;
    }

    public static void SelectPrevious()
    {
        int count = AvailableVehicles.Length;
        int prevIndex = ((int)SelectedVehicle - 1 + count) % count;
        SelectedVehicle = (VehicleType)prevIndex;
    }
}
