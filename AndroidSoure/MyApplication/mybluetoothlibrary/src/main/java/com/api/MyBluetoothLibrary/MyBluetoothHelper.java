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

//    @Override
//    protected void onCreate(Bundle savedInstanceState) {
//        Toast.makeText(context, "onCreate", Toast.LENGTH_LONG).show();
//        super.onCreate(savedInstanceState);
//    }

//    @Override
//    protected void onDestroy() {
//        super.onDestroy();
//        this.unregisterReceiver(this.receiver);
//    }

//    @Override
//    public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults) {
//        Toast.makeText(context, "onRequestPermissionsResult", Toast.LENGTH_LONG).show();
//        if (requestCode == REQUEST_BLUETOOTH_PERMISSION_CODE && grantResults.length != 0 && grantResults[0] == PackageManager.PERMISSION_GRANTED) {
//            this.TryEnableBluetooth();
//        }
//
//        if (requestCode == REQUEST_LOCATION_PERMISSION_CODE && grantResults.length != 0 && grantResults[0] == PackageManager.PERMISSION_GRANTED) {
//            this.TryEnableBluetoothLocation();
//        }
//
//        super.onRequestPermissionsResult(requestCode, permissions, grantResults);
//    }

    public void registerReceiver() {
        if (!receiverManager.isReceiverRegistered(this.receiver)) {
            IntentFilter filter = new IntentFilter(BluetoothDevice.ACTION_FOUND);
//            activity.registerReceiver(this.receiver, filter);
            receiverManager.registerReceiver(this.receiver, filter);
        }
    }

    public void unregisterReceiver() {
        receiverManager.unregisterReceiver(this.receiver);
    }

    public boolean IsBluetoothAvailable() {
        if (VERSION.SDK_INT >= Build.VERSION_CODES.M) {
            BluetoothManager bluetoothManager = context.getSystemService(BluetoothManager.class);
            if (bluetoothManager == null) {
                Toast.makeText(this.context, "BluetoothManager Is Null", Toast.LENGTH_LONG).show();
                return false;
            }

            this.bluetoothAdapter = bluetoothManager.getAdapter();
        }

        if (this.bluetoothAdapter == null && VERSION.SDK_INT < Build.VERSION_CODES.S) {
            this.bluetoothAdapter = BluetoothAdapter.getDefaultAdapter();
        }

        if (this.bluetoothAdapter != null) return true;

        Toast.makeText(this.context, "BluetoothAdapter Is Null", Toast.LENGTH_LONG).show();
        return false;
    }

    public boolean IsBluetoothEnabled() {
        if (this.bluetoothAdapter == null) return false;
        if (!this.IsBluetoothAvailable()) return false;

        return this.bluetoothAdapter.isEnabled();
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

    public boolean TryEnableBluetooth() {
        if (this.bluetoothAdapter == null) return false;
        if (!this.RequestBluetoothPermission()) return false;

        if (this.bluetoothAdapter.isEnabled()) return true;

        this.bluetoothAdapter.enable();

        if (this.bluetoothAdapter.isEnabled()) return true;

        Intent enableBtIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
        enableBtIntent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        this.context.startActivity(enableBtIntent);
        return false;
    }

    public void TryDisableBluetooth() {
        if (this.bluetoothAdapter == null) return;
        if (!this.RequestBluetoothPermission()) return;

        if (!this.bluetoothAdapter.isEnabled()) return;

        if (ActivityCompat.checkSelfPermission(this.context, Manifest.permission.BLUETOOTH_CONNECT) != PackageManager.PERMISSION_GRANTED)
            return;
        this.bluetoothAdapter.disable();

        if (!this.bluetoothAdapter.isEnabled()) return;

        Intent enableBtIntent = new Intent("android.bluetooth.adapter.action.REQUEST_DISABLE");
        enableBtIntent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        this.context.startActivity(enableBtIntent);
    }

    public boolean TryEnableBluetoothLocation() {
        if (!this.RequestBluetoothScanPermission()) return false;

        if (VERSION.SDK_INT < Build.VERSION_CODES.M) return true;

        LocationManager locationManager = (LocationManager) this.context.getSystemService(Context.LOCATION_SERVICE);

        if (locationManager == null) return false;

        if (locationManager.isProviderEnabled(LocationManager.GPS_PROVIDER)) return true;

        Intent enableGpsIntent = new Intent(Settings.ACTION_LOCATION_SOURCE_SETTINGS);
        enableGpsIntent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        this.context.startActivity(enableGpsIntent);
        return false;
    }

    public HashMap<String, String> GetBondedDevices() {
        this.bondedDevices.clear();
        if (!this.TryEnableBluetooth()) return this.bondedDevices;

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

    public boolean ScanClassicDevices() {
        if (this.bluetoothAdapter == null) return false;
        if (!this.TryEnableBluetooth()) return false;
        if (!this.RequestBluetoothScanPermission()) return false;

        if (ActivityCompat.checkSelfPermission(this.context, Manifest.permission.BLUETOOTH_SCAN) != PackageManager.PERMISSION_GRANTED)
            return false;

        if (this.bluetoothAdapter.isDiscovering()) this.bluetoothAdapter.cancelDiscovery();

        this.notBondedDevices.clear();
        if (!this.TryEnableBluetoothLocation()) return false;

        return this.bluetoothAdapter.startDiscovery();
    }

    public boolean Bond(String address) {
        if (this.bluetoothAdapter == null) return false;
        if (!this.TryEnableBluetooth()) return false;
        if (ActivityCompat.checkSelfPermission(this.context, Manifest.permission.BLUETOOTH_SCAN) != PackageManager.PERMISSION_GRANTED)
            return false;

        if (this.bluetoothAdapter.isDiscovering()) this.bluetoothAdapter.cancelDiscovery();

        if (VERSION.SDK_INT >= Build.VERSION_CODES.KITKAT) {
            BluetoothDevice bluetoothDevice = this.bluetoothAdapter.getRemoteDevice(address);
            if (bluetoothDevice != null) {
                return bluetoothDevice.createBond();
            }
        }
        return false;
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