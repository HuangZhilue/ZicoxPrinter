var printer = null;//api.require("myzicoxprinter2");

var deviceMacAddress = "00:11:22:33:44:55";

/**
 * 搜索指定条件的设备
 */
function searchDevices(callback) {
  let searchDevices = { Device: "CC3", DeviceRegexStr: "^(C[CS]\\d+)" };
  // searchDevices = { Device: "CC3" }; // 搜索蓝牙设备名称中包含CC3的设备
  // searchDevices = { DeviceRegexStr: "^(C[CS]\\d+)" }; // 根据正则表达式搜索
  // searchDevices = {} 或者 "" 或者 null; // 搜索所有设备
  printer.getDevices(searchDevices, function (r) {
    // 该方法会自动判断蓝牙是否可用、是否开启了蓝牙、尝试自动打开蓝牙
    /*
      r = {
        "devices": "{CC4_1234=00:11:22:33:44:55, CC3_1234=66:77:88:99:AA:BB}",
        "message": "蓝牙不可用" // "尝试打开蓝牙", "当前设备没有匹配到蓝牙设备", Java_Exception_Message
      }
     */
    console.log(JSON.stringify(r));
    let d = r.devices;
    let arr = /\w{2}:\w{2}:\w{2}:\w{2}:\w{2}:\w{2}/g.exec(d);
    deviceMacAddress = arr[0]; // 第一个设备
    !callback || callback(r);
  });
}

/**
 * 打印
 */
function print(callback) {
  let printObj = {
    Address: deviceMacAddress, // 打印机的MAC地址
    JSONArray: [
      // 以下能看到的数值都是“默认值”
      {
        drawType: "DRAWBOX",
        lineWidth: 1,
        top_left_x: 0,
        top_left_y: 0,
        bottom_right_x: 0,
        bottom_right_y: 0,
      },
      {
        drawType: "DRAWLINE",
        lineWidth: 1,
        start_x: 0,
        start_y: 0,
        end_x: 0,
        end_y: 0,
      },
      {
        drawType: "DRAWTEXT1",
        text_x: 0,
        text_y: 0,
        text: "",
        fontSize: "EN7", // 有效值[EN0,EN1,EN2,EN3,EN4,EN5,EN6,EN7,CN16,CN24]，其它值默认变更为EN0
        rotate: "T", // 有效值[T,VT,T90,T180,T270]（其中的VT与T90相同），其它值默认变更为T
        bold: false,
        reverse: false,
        underline: false,
      },
      {
        drawType: "DRAWTEXT2",
        text_x: 0,
        text_y: 0,
        text: "",
        width: 0,
        height: 0,
        fontSize: "EN6", // 有效值[EN0,EN1,EN2,EN3,EN4,EN5,EN6,EN7,CN16,CN24]，其它值默认变更为EN0
        rotate: "T", // 有效值[T,VT,T90,T180,T270]（其中的VT与T90相同），其它值默认变更为T
        bold: false,
        reverse: false,
        underline: false,
      },
      {
        drawType: "DRAWBARCODE",
        start_x: 0,
        start_y: 0,
        type: "CODE128", // 有效值[CODE39,CODE128,CODE93,CODABAR,EAN8,EAN13,UPC_A,UPC_E,I2OF5]，其它值默认变更为CODE128
        height: 0,
        text: "",
        linewidth: 1,
        rotate: false,
      },
      {
        drawType: "DRAWQRCODE",
        start_x: 0,
        start_y: 0,
        text: "",
        ver: 0, // 相当于图形的放大倍数，取值范围为[0-32]（默认值为0）
      },
      {
        drawType: "DRAWGRAPHIC",
        start_x: 0,
        start_y: 0,
        width_limit: 0, // 在生成图片时，用于限制图片宽度，防止超宽图片无法打印（设备死机重启）。0表示不限制（默认值为0）
        height_limit: 0, // 在生成图片时，用于限制图片高度，防止超高图片无法打印（设备死机重启）。0表示不限制（默认值为0）
        bmp_size_w_percentage: 100, // 图片宽度缩放百分比，建议的赋值方式为【打印机页面宽度 / 图片宽度 * 100】（默认值为100（即100%，不缩放））
        bmp_size_h_percentage: 100, // 图片高度缩放百分比，建议的赋值方式为【打印机页面高度 / 图片高度 * 100】（或者同为 图片宽度缩放百分比）（默认值为100（即100%，不缩放））
        rotate: 0, // 旋转角度，建议的取值范围 [-360 - 360] （单位：度°）（默认值为0）
        threshold: 128, // 取值范围为[0-255]（默认值为128）
        dithering_type: "SimpleThreshold", // 如果不是有效值，默认变更为 SimpleThreshold
        base64: "", // 图片的base64编码字符串，不包含有前缀字符串（"data:image/jpeg;base64,"等）
        /*
      支持的dithering_type如下:
        None,
        Ordered2By2Bayer,
        Ordered3By3Bayer,
        Ordered4By4Bayer,
        Ordered8By8Bayer,
        FloydSteinberg,
        JarvisJudiceNinke,
        Sierra,
        TwoRowSierra,
        SierraLite,
        Atkinson,
        Stucki,
        Burkes,
        FalseFloydSteinberg,
        SimpleLeftToRightErrorDiffusion,
        RandomDithering,
        SimpleThreshold
      */
      },
      {
        drawType: "DRAWBIGGRAPHIC", // ！注意！建议单独使用该方法打印大图，不建议再包含其它打印方法！
        start_x: 0,
        start_y: 0,
        width_limit: 0, // 在生成图片时，用于限制图片宽度，防止超宽图片无法打印（设备死机重启）。0表示不限制（默认值为0）
        height_limit: 0, // 在生成图片时，用于限制图片高度，防止超高图片无法打印（设备死机重启）。0表示不限制（默认值为0）
        bmp_size_w_percentage: 100, // 图片宽度缩放百分比，建议的赋值方式为【打印机页面宽度 / 图片宽度 * 100】（默认值为100（即100%，不缩放））
        bmp_size_h_percentage: 100, // 图片高度缩放百分比，建议的赋值方式为【打印机页面高度 / 图片高度 * 100】（或者同为 图片宽度缩放百分比）（默认值为100（即100%，不缩放））
        rotate: 0, // 旋转角度，建议的取值范围 [-360 - 360] （单位：度°）（默认值为0）
        threshold: 128, // 取值范围为[0-255]（默认值为128）
        dithering_type: "SimpleThreshold", // 如果不是有效值，默认变更为 SimpleThreshold
        base64: "", // 图片的base64编码字符串，不包含有前缀字符串（"data:image/jpeg;base64,"等）
        /*
      支持的dithering_type如下:
        None,
        Ordered2By2Bayer,
        Ordered3By3Bayer,
        Ordered4By4Bayer,
        Ordered8By8Bayer,
        FloydSteinberg,
        JarvisJudiceNinke,
        Sierra,
        TwoRowSierra,
        SierraLite,
        Atkinson,
        Stucki,
        Burkes,
        FalseFloydSteinberg,
        SimpleLeftToRightErrorDiffusion,
        RandomDithering,
        SimpleThreshold
      */
      },
      // 以上能看到的数值都是“默认值”
    ],
    PageWight: 1400, // 该宽度为打印机的可打印宽度, 大于该宽度的内容不会打印。CC4最大宽度约为 800, CC3最大宽度为 576。
    PageHeight: 100, // 该宽度为打印机的可打印高度, 大于该高度的内容不会打印。使用 DRAWBIGGRAPHIC 打印大图时，可以设置为 0
  };

  printer.print(printObj, function (result) {
    /*
      result = {
        "message": "蓝牙地址为空", // "打印内容为空", Java_Exception_Message
        "status": "success" // "SDK初始化失败", "蓝牙打印机连接失败", "无法打印打印机缺纸", "无法打印打印机开盖", "打印失败"
      }
     */
    console.log(JSON.stringify(result));
    !callback || callback(r);
  });
}

/**
 * 根据CPCL指令来打印
 */
function printCommand(callback) {
  const command =
    "! 0 200 200 160 1\r\n" + // 注意\r\n换行符
    "PAGE-WIDTH 240\r\n" +
    "TEXT 0 2 80 80 HELLO WORLD\r\n" +
    "PRINT\r\n" +
    ""; // 最后有一行空白行

  let printObj = {
    Address: deviceMacAddress,
    Command: command, // CPCL指令
  };
  printer.printCommand(printObj, function (result) {
    /*
      result = {
        "message": "蓝牙地址为空", // "打印命令为空", Java_Exception_Message
        "status": "success" // "SDK初始化失败", "蓝牙打印机连接失败", "无法打印打印机缺纸", "无法打印打印机开盖", "打印失败"
      }
     */
    console.log(JSON.stringify(result));
    !callback || callback(result);
  });
}

/**
 * 使用内置的图片处理方法来处理一张图片（并返回新图片的base64编码字符串）
 */
function processImageToBase64(imgBase64String, callback) {
  // 图片的base64编码字符串，不包含有前缀字符串（"data:image/jpeg;base64,"等）

  let imgObj = {
    start_x: 0,
    start_y: 0,
    width_limit: 0, // 在生成图片时，用于限制图片宽度，防止超宽图片无法打印（设备死机重启）。0表示不限制（默认值为0）
    height_limit: 0, // 在生成图片时，用于限制图片高度，防止超高图片无法打印（设备死机重启）。0表示不限制（默认值为0）
    bmp_size_w_percentage: 100, // 图片宽度缩放百分比，建议的赋值方式为【打印机页面宽度 / 图片宽度 * 100】（默认值为100（即100%，不缩放））
    bmp_size_h_percentage: 100, // 图片高度缩放百分比，建议的赋值方式为【打印机页面高度 / 图片高度 * 100】（或者同为 图片宽度缩放百分比）（默认值为100（即100%，不缩放））
    rotate: 0, // 旋转角度，建议的取值范围 [-360 - 360] （单位：度°）（默认值为0）
    threshold: 128, // 取值范围为[0-255]（默认值为128）
    dithering_type: "SimpleThreshold", // 如果不是有效值，默认变更为 SimpleThreshold
    base64: imgBase64String, // 图片的base64编码字符串，不包含有前缀字符串（"data:image/jpeg;base64,"等）
    /*
  支持的dithering_type如下:
    None,
    Ordered2By2Bayer,
    Ordered3By3Bayer,
    Ordered4By4Bayer,
    Ordered8By8Bayer,
    FloydSteinberg,
    JarvisJudiceNinke,
    Sierra,
    TwoRowSierra,
    SierraLite,
    Atkinson,
    Stucki,
    Burkes,
    FalseFloydSteinberg,
    SimpleLeftToRightErrorDiffusion,
    RandomDithering,
    SimpleThreshold
  */
  };
  printer.processImageToBase64(imgObj, function (result) {
    /*
      result = {
        "newImageBase64": "处理完成之后的新的图片的base64字符串",
        "message": Java_Exception_Message
      }
     */
    console.log(JSON.stringify(result));
    !callback || callback(result);
  });
}

/**
 * 专门打印一个尺子的方法，其中尺子的每一个条纹间的间隔都是1mm（8像素点）
 */
function printCPCLRuler(callback) {
  let printObj = {
    Address: deviceMacAddress,
    Width: 1000,
    Height: 120,
  };
  printer.printCPCLRuler(printObj, function (result) {
    /*
      result = {
        "message": "蓝牙地址为空", // Java_Exception_Message
        "status": "success" // "SDK初始化失败", "蓝牙打印机连接失败", "无法打印打印机缺纸", "无法打印打印机开盖", "打印失败"
      }
     */
    console.log(JSON.stringify(result));
    !callback || callback(result);
  });
}
