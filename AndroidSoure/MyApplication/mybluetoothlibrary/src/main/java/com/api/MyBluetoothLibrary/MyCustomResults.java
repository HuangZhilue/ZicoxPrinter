package com.api.MyBluetoothLibrary;

public enum MyCustomResults {
    SUCCESS(0),
    FAILURE(1),
    CANCELED(2),
    BluetoothAdapterIsNull(3),
    TryEnableBluetoothFailed(4),
    RequestBluetoothScanPermissionFailed(5),
    CheckSelfPermissionFailed(6),
    TryEnableBluetoothLocationFailed(7),
    StartDiscoveryFailed(8),
    CancelDiscoveryFailed(9),
    CreateBondFailed(10),
    BluetoothManagerIsNull(11),
    RequestBluetoothPermissionFailed(12),
    TryStartActivity(13),
    LocationManagerIsNull(14),;
    private final int value;

    MyCustomResults(int value) {
        this.value = value;
    }

    public int getValue() {
        return value;
    }
}
