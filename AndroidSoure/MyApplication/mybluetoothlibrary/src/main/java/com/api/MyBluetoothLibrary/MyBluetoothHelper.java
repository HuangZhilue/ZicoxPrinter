package com.api.MyBluetoothLibrary;

import android.Manifest;
import android.annotation.SuppressLint;
import android.app.Activity;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothManager;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.pm.PackageManager;
import android.location.LocationManager;
import android.os.Build;
import android.os.Build.VERSION;
import android.provider.Settings;
import android.widget.Toast;

import androidx.core.app.ActivityCompat;

import java.util.HashMap;
import java.util.Set;

public class MyBluetoothHelper {
    @SuppressLint("StaticFieldLeak")
    private static volatile MyBluetoothHelper instance;
    private BluetoothAdapter bluetoothAdapter;
    private final HashMap<String, String> bondedDevices = new HashMap<>();
    private final HashMap<String, String> notBondedDevices = new HashMap<>();
    private final Context context;
    private final Activity activity;
    private final ReceiverManager receiverManager;
    private final BroadcastReceiver receiver;
    private static final int REQUEST_BLUETOOTH_PERMISSION_CODE = 1;
    private static final int REQUEST_LOCATION_PERMISSION_CODE = 2;

    public void registerReceiver() {
        if (!receiverManager.isReceiverRegistered(this.receiver)) {
            IntentFilter filter = new IntentFilter(BluetoothDevice.ACTION_FOUND);
            receiverManager.registerReceiver(this.receiver, filter);
        }
    }

    public void unregisterReceiver() {
        receiverManager.unregisterReceiver(this.receiver);
    }

    public MyCustomResults IsBluetoothAvailable() {
        if (VERSION.SDK_INT >= Build.VERSION_CODES.M) {
            BluetoothManager bluetoothManager = context.getSystemService(BluetoothManager.class);
            if (bluetoothManager == null) {
                Toast.makeText(this.context, "BluetoothManager Is Null", Toast.LENGTH_LONG).show();
                return MyCustomResults.BluetoothManagerIsNull;
            }

            this.bluetoothAdapter = bluetoothManager.getAdapter();
        }

        if (this.bluetoothAdapter == null && VERSION.SDK_INT < Build.VERSION_CODES.S) {
            this.bluetoothAdapter = BluetoothAdapter.getDefaultAdapter();
        }

        if (this.bluetoothAdapter != null) return MyCustomResults.SUCCESS;

        Toast.makeText(this.context, "BluetoothAdapter Is Null", Toast.LENGTH_LONG).show();
        return MyCustomResults.BluetoothAdapterIsNull;
    }

    public MyCustomResults IsBluetoothEnabled() {
        if (this.bluetoothAdapter == null) return MyCustomResults.BluetoothAdapterIsNull;
        MyCustomResults result = this.IsBluetoothAvailable();
        if (result != MyCustomResults.SUCCESS) return result;

        return this.bluetoothAdapter.isEnabled() ? MyCustomResults.SUCCESS : MyCustomResults.FAILURE;
    }

    public boolean RequestBluetoothPermission() {
        boolean permission = true;
        if (VERSION.SDK_INT >= Build.VERSION_CODES.S) {
            if (this.context.checkSelfPermission(Manifest.permission.BLUETOOTH_CONNECT) != PackageManager.PERMISSION_GRANTED) {
                permission = false;
                ActivityCompat.requestPermissions(this.activity, new String[]{Manifest.permission.BLUETOOTH_CONNECT}, REQUEST_BLUETOOTH_PERMISSION_CODE);
            }

            if (this.context.checkSelfPermission(Manifest.permission.BLUETOOTH_SCAN) != PackageManager.PERMISSION_GRANTED) {
                permission = false;
                ActivityCompat.requestPermissions(this.activity, new String[]{Manifest.permission.BLUETOOTH_SCAN}, REQUEST_BLUETOOTH_PERMISSION_CODE);
            }
        }

        if (VERSION.SDK_INT >= Build.VERSION_CODES.M) {
            if (this.context.checkSelfPermission(Manifest.permission.BLUETOOTH) != PackageManager.PERMISSION_GRANTED) {
                permission = false;
                ActivityCompat.requestPermissions(this.activity, new String[]{Manifest.permission.BLUETOOTH}, REQUEST_BLUETOOTH_PERMISSION_CODE);
            }

            if (this.context.checkSelfPermission(Manifest.permission.BLUETOOTH_ADMIN) != PackageManager.PERMISSION_GRANTED) {
                permission = false;
                ActivityCompat.requestPermissions(this.activity, new String[]{Manifest.permission.BLUETOOTH_ADMIN}, REQUEST_BLUETOOTH_PERMISSION_CODE);
            }
        }

        return permission;
    }

    public boolean RequestBluetoothScanPermission() {
        boolean permission = true;
        if (VERSION.SDK_INT < Build.VERSION_CODES.M) return permission;

        if (this.context.checkSelfPermission(Manifest.permission.ACCESS_FINE_LOCATION) != PackageManager.PERMISSION_GRANTED) {
            permission = false;
            ActivityCompat.requestPermissions(this.activity, new String[]{Manifest.permission.ACCESS_FINE_LOCATION}, REQUEST_LOCATION_PERMISSION_CODE);
        }

        if (this.context.checkSelfPermission(Manifest.permission.ACCESS_COARSE_LOCATION) != PackageManager.PERMISSION_GRANTED) {
            permission = false;
            ActivityCompat.requestPermissions(this.activity, new String[]{Manifest.permission.ACCESS_COARSE_LOCATION}, REQUEST_LOCATION_PERMISSION_CODE);
        }

        return permission;
    }

    public MyCustomResults TryEnableBluetooth() {
        if (this.bluetoothAdapter == null) return MyCustomResults.BluetoothAdapterIsNull;
        if (!this.RequestBluetoothPermission())
            return MyCustomResults.RequestBluetoothPermissionFailed;

        if (this.bluetoothAdapter.isEnabled()) return MyCustomResults.SUCCESS;

        this.bluetoothAdapter.enable();

        if (this.bluetoothAdapter.isEnabled()) return MyCustomResults.SUCCESS;

        Intent enableBtIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
        enableBtIntent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        this.context.startActivity(enableBtIntent);
        return MyCustomResults.TryStartActivity;
    }

    public MyCustomResults TryDisableBluetooth() {
        if (this.bluetoothAdapter == null) return MyCustomResults.BluetoothAdapterIsNull;
        if (!this.RequestBluetoothPermission())
            return MyCustomResults.RequestBluetoothPermissionFailed;

        if (!this.bluetoothAdapter.isEnabled()) return MyCustomResults.SUCCESS;

        this.bluetoothAdapter.disable();

        if (!this.bluetoothAdapter.isEnabled()) return MyCustomResults.SUCCESS;

        Intent enableBtIntent = new Intent("android.bluetooth.adapter.action.REQUEST_DISABLE");
        enableBtIntent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        this.context.startActivity(enableBtIntent);
        return MyCustomResults.TryStartActivity;
    }

    public MyCustomResults TryEnableBluetoothLocation() {
        if (!this.RequestBluetoothScanPermission())
            return MyCustomResults.RequestBluetoothScanPermissionFailed;

        if (VERSION.SDK_INT < Build.VERSION_CODES.M) return MyCustomResults.SUCCESS;

        LocationManager locationManager = (LocationManager) this.context.getSystemService(Context.LOCATION_SERVICE);

        if (locationManager == null) return MyCustomResults.LocationManagerIsNull;

        if (locationManager.isProviderEnabled(LocationManager.GPS_PROVIDER))
            return MyCustomResults.SUCCESS;

        Intent enableGpsIntent = new Intent(Settings.ACTION_LOCATION_SOURCE_SETTINGS);
        enableGpsIntent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        this.context.startActivity(enableGpsIntent);
        return MyCustomResults.TryStartActivity;
    }

    public HashMap<String, String> GetBondedDevices() {
        this.bondedDevices.clear();
        if (this.TryEnableBluetooth() != MyCustomResults.SUCCESS) return this.bondedDevices;

        Set<BluetoothDevice> pairedDevices = this.bluetoothAdapter.getBondedDevices();

        for (BluetoothDevice device : pairedDevices) {
            if (device == null || device.getName() == null || device.getName().isEmpty() || device.getAddress() == null || device.getAddress().isEmpty())
                continue;
            this.bondedDevices.put(device.getName(), device.getAddress());
        }

        if (this.bondedDevices.isEmpty()) {
            Toast.makeText(context, "当前设备没有匹配到蓝牙设备", Toast.LENGTH_LONG).show();
        }

        return this.bondedDevices;
    }

    public HashMap<String, String> FoundClassicDevices() {
        return this.notBondedDevices;
    }

    public MyCustomResults ScanClassicDevices() {
        if (this.bluetoothAdapter == null) return MyCustomResults.BluetoothAdapterIsNull;
        MyCustomResults result = this.TryEnableBluetooth();
        if (result != MyCustomResults.SUCCESS) return result;
//        if (!this.TryEnableBluetooth()) return MyCustomResults.TryEnableBluetoothFailed;
        if (!this.RequestBluetoothScanPermission())
            return MyCustomResults.RequestBluetoothScanPermissionFailed;

        if (VERSION.SDK_INT >= Build.VERSION_CODES.S) {
            if (ActivityCompat.checkSelfPermission(this.context, Manifest.permission.BLUETOOTH_SCAN) != PackageManager.PERMISSION_GRANTED)
                return MyCustomResults.CheckSelfPermissionFailed;
        }

        if (this.bluetoothAdapter.isDiscovering()) this.bluetoothAdapter.cancelDiscovery();

        this.notBondedDevices.clear();
        result = this.TryEnableBluetoothLocation();
        if (result != MyCustomResults.SUCCESS) return result;
//        if (!this.TryEnableBluetoothLocation())
//            return MyCustomResults.TryEnableBluetoothLocationFailed;

        return this.bluetoothAdapter.startDiscovery() ? MyCustomResults.SUCCESS : MyCustomResults.StartDiscoveryFailed;
    }

    public MyCustomResults Bond(String address) {
        if (this.bluetoothAdapter == null) return MyCustomResults.BluetoothAdapterIsNull;
        MyCustomResults result = this.TryEnableBluetooth();
        if (result != MyCustomResults.SUCCESS) return result;
//        if (!this.TryEnableBluetooth()) return MyCustomResults.TryEnableBluetoothFailed;

        if (VERSION.SDK_INT >= Build.VERSION_CODES.M) {
            if (ActivityCompat.checkSelfPermission(this.context, Manifest.permission.BLUETOOTH) != PackageManager.PERMISSION_GRANTED)
                return MyCustomResults.CheckSelfPermissionFailed;
            if (ActivityCompat.checkSelfPermission(this.context, Manifest.permission.BLUETOOTH_ADMIN) != PackageManager.PERMISSION_GRANTED)
                return MyCustomResults.CheckSelfPermissionFailed;
        }

        if (this.bluetoothAdapter.isDiscovering()) this.bluetoothAdapter.cancelDiscovery();

        if (VERSION.SDK_INT >= Build.VERSION_CODES.KITKAT) {
            BluetoothDevice bluetoothDevice = this.bluetoothAdapter.getRemoteDevice(address);
            if (bluetoothDevice != null) {
                return bluetoothDevice.createBond() ? MyCustomResults.SUCCESS : MyCustomResults.CreateBondFailed;
            }
        }
        return MyCustomResults.FAILURE;
    }

    private MyBluetoothHelper(Context context, Activity activity) {
//        super();
        this.context = context;
        this.activity = activity;
        receiverManager = ReceiverManager.init(context);
        receiver = new BroadcastReceiver() {
            public void onReceive(Context context, Intent intent) {
                String action = String.valueOf(intent.getAction());
                if (action.equals(BluetoothDevice.ACTION_FOUND)) {

                    BluetoothDevice device;
                    if (VERSION.SDK_INT < Build.VERSION_CODES.TIRAMISU) {
                        device = (BluetoothDevice) intent.getParcelableExtra(BluetoothDevice.EXTRA_DEVICE);
                    } else {
                        device = (BluetoothDevice) intent.getParcelableExtra(BluetoothDevice.EXTRA_DEVICE, BluetoothDevice.class);
                    }

                    if (device == null || device.getName() == null || device.getName().isEmpty() || device.getAddress() == null || device.getAddress().isEmpty())
                        return;
                    String deviceName = device.getName();
                    String deviceHardwareAddress = device.getAddress();
                    notBondedDevices.put(deviceName, deviceHardwareAddress);
                }
            }
        };
    }

    public static MyBluetoothHelper Init(Context context, Activity activity) {
        if (instance == null) {
            synchronized (MyBluetoothHelper.class) {
                if (instance == null) {
                    instance = new MyBluetoothHelper(context, activity);
                }
            }
        }

        return instance;
    }

    public static MyBluetoothHelper GetInstance() {
        if (instance == null)
            throw new NullPointerException("MyBluetoothHelper is null, please call MyBluetoothHelper.Init(context, activity) first");
        return instance;
    }
}