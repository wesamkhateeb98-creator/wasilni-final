// ignore_for_file: avoid_print

import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:signalr_netcore/signalr_client.dart';

// ─── Config ───────────────────────────────────────────────────────────────────
const String _baseUrl = 'http://localhost:5054';
const String _hubUrl  = '$_baseUrl/tracking-hub';

// ─── Models ───────────────────────────────────────────────────────────────────

class DriverBusInfo {
  final int     busId;
  final String  plate;
  final String  color;
  final int     type;
  final int     status;   // 0=Inactive, 1=Active
  final int?    lineId;
  final String? lineName;

  DriverBusInfo.fromJson(Map<String, dynamic> j)
      : busId    = j['busId'],
        plate    = j['plate'],
        color    = j['color'],
        type     = j['type'],
        status   = j['status'],
        lineId   = j['lineId'],
        lineName = j['lineName'];
}

class ActiveBusPayload {
  final int     busId;
  final String  plate;
  final int     lineId;
  final String  lineName;
  final String  status;
  final double? latitude;
  final double? longitude;
  final int     anonymousCount;

  ActiveBusPayload.fromJson(Map<String, dynamic> j)
      : busId         = j['busId'],
        plate         = j['plate'],
        lineId        = j['lineId'],
        lineName      = j['lineName'],
        status        = j['status'],
        latitude      = (j['latitude'] as num?)?.toDouble(),
        longitude     = (j['longitude'] as num?)?.toDouble(),
        anonymousCount= j['anonymousCount'];
}

class BookingPayload {
  final int    id;
  final int    lineId;
  final int    passengerId;
  final String date;
  final double latitude;
  final double longitude;
  final String status;
  final String createdAt;

  BookingPayload.fromJson(Map<String, dynamic> j)
      : id          = j['id'],
        lineId      = j['lineId'],
        passengerId = j['passengerId'],
        date        = j['date'],
        latitude    = (j['latitude'] as num).toDouble(),
        longitude   = (j['longitude'] as num).toDouble(),
        status      = j['status'],
        createdAt   = j['createdAt'];
}

class BookingStatusPayload {
  final int    bookingId;
  final String status;

  BookingStatusPayload.fromJson(Map<String, dynamic> j)
      : bookingId = j['bookingId'],
        status    = j['status'];
}

// ─── DriverService ────────────────────────────────────────────────────────────

class DriverService {
  final String phone;
  final String password;

  String?           _token;
  HubConnection?    _hub;
  DriverBusInfo?    _busInfo;

  DriverService({required this.phone, required this.password});

  // ── 1. Login ────────────────────────────────────────────────────────────────
  Future<void> login() async {
    final res = await http.post(
      Uri.parse('$_baseUrl/api/v1.0/accounts/login'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({'phoneNumber': phone, 'password': password}),
    );

    if (res.statusCode != 200) {
      throw Exception('Login failed: ${res.statusCode} ${res.body}');
    }

    final data = jsonDecode(res.body);
    _token = data['token'] ?? data['accessToken'];
    print('[Auth] ✅ Logged in — token: ${_token!.substring(0, 20)}…');
  }

  // ── 2. Get Bus Info (cached on server) ──────────────────────────────────────
  Future<void> loadBusInfo() async {
    final res = await http.get(
      Uri.parse('$_baseUrl/api/v1.0/buses/my-info'),
      headers: {'Authorization': 'Bearer $_token'},
    );

    if (res.statusCode != 200) {
      throw Exception('GetBusInfo failed: ${res.statusCode} ${res.body}');
    }

    _busInfo = DriverBusInfo.fromJson(jsonDecode(res.body));
    print('[BusInfo] ✅ Bus #${_busInfo!.busId} — ${_busInfo!.plate} — Line: ${_busInfo!.lineName}');
  }

  // ── 3. Connect Hub ──────────────────────────────────────────────────────────
  Future<void> connectHub() async {
    _hub = HubConnectionBuilder()
        .withUrl(
          _hubUrl,
          options: HttpConnectionOptions(
            accessTokenFactory: () async => _token!,
          ),
        )
        .withAutomaticReconnect()
        .build();

    _registerEvents();

    await _hub!.start();
    print('[Hub] ✅ Connected');
  }

  // ── 4. Register Hub Events ──────────────────────────────────────────────────
  void _registerEvents() {
    // OnBusActivated — يُرسل لكروب line-{id} عند تفعيل باص
    _hub!.on('OnBusActivated', (args) {
      if (args == null || args.isEmpty) return;
      final payload = ActiveBusPayload.fromJson(
        Map<String, dynamic>.from(args[0] as Map),
      );
      print('[Event] 🟢 OnBusActivated — Bus #${payload.busId} on Line #${payload.lineId} (${payload.lineName})');
      _onBusActivated(payload);
    });

    // OnBusDeactivated — يُرسل لكروب line-{id} عند إيقاف باص
    _hub!.on('OnBusDeactivated', (args) {
      if (args == null || args.isEmpty) return;
      final busId = (args[0] as Map)['busId'] as int;
      print('[Event] 🔴 OnBusDeactivated — Bus #$busId');
      _onBusDeactivated(busId);
    });

    // OnLocationUpdated — يُرسل لكروب line-{id} عند تحديث الموقع
    _hub!.on('OnLocationUpdated', (args) {
      if (args == null || args.isEmpty) return;
      final m = Map<String, dynamic>.from(args[0] as Map);
      print('[Event] 📍 OnLocationUpdated — Bus #${m['busId']} → (${m['latitude']}, ${m['longitude']})');
    });

    // OnBookingAdded — يُرسل لكروب line-booking-{id} عند حجز جديد
    _hub!.on('OnBookingAdded', (args) {
      if (args == null || args.isEmpty) return;
      final booking = BookingPayload.fromJson(
        Map<String, dynamic>.from(args[0] as Map),
      );
      print('[Event] 📌 OnBookingAdded — Booking #${booking.id} | Passenger #${booking.passengerId} @ (${booking.latitude}, ${booking.longitude})');
      _onBookingAdded(booking);
    });

    // OnBookingStatusChanged — يُرسل لكروب line-booking-{id} عند confirm/noshow/cancel
    _hub!.on('OnBookingStatusChanged', (args) {
      if (args == null || args.isEmpty) return;
      final payload = BookingStatusPayload.fromJson(
        Map<String, dynamic>.from(args[0] as Map),
      );
      print('[Event] 🔄 OnBookingStatusChanged — Booking #${payload.bookingId} → ${payload.status}');
      _onBookingStatusChanged(payload);
    });
  }

  // ── 5. Activate Bus ─────────────────────────────────────────────────────────
  Future<void> activateBus() async {
    print('[Hub] ▶ Invoking ActiveBus…');
    await _hub!.invoke('ActiveBus');
    print('[Hub] ✅ Bus activated');
  }

  // ── 6. Deactivate Bus ───────────────────────────────────────────────────────
  Future<void> deactivateBus() async {
    print('[Hub] ⏹ Invoking InactiveBus…');
    await _hub!.invoke('InactiveBus');
    print('[Hub] ✅ Bus deactivated');
  }

  // ── 7. Update Location ──────────────────────────────────────────────────────
  Future<void> updateLocation(double lat, double lng) async {
    await _hub!.invoke('UpdateLocation', args: [
      {'latitude': lat, 'longitude': lng},
    ]);
    print('[Hub] 📍 Location sent → ($lat, $lng)');
  }

  // ── 8. Confirm Booking ──────────────────────────────────────────────────────
  Future<void> confirmBooking(int bookingId) async {
    final res = await http.put(
      Uri.parse('$_baseUrl/api/v1.0/bookings/$bookingId/confirm'),
      headers: {'Authorization': 'Bearer $_token'},
    );

    if (res.statusCode != 200) {
      throw Exception('ConfirmBooking failed: ${res.statusCode} ${res.body}');
    }

    print('[REST] ✅ Booking #$bookingId confirmed');
  }

  // ── 9. No-Show Booking ──────────────────────────────────────────────────────
  Future<void> markNoShow(int bookingId) async {
    final res = await http.put(
      Uri.parse('$_baseUrl/api/v1.0/bookings/$bookingId/no-show'),
      headers: {'Authorization': 'Bearer $_token'},
    );

    if (res.statusCode != 200) {
      throw Exception('MarkNoShow failed: ${res.statusCode} ${res.body}');
    }

    print('[REST] ✅ Booking #$bookingId marked as no-show');
  }

  // ── 10. Get Nearby Bookings ─────────────────────────────────────────────────
  Future<List<BookingPayload>> getNearbyBookings() async {
    final res = await http.get(
      Uri.parse('$_baseUrl/api/v1.0/bookings/nearby'),
      headers: {'Authorization': 'Bearer $_token'},
    );

    if (res.statusCode != 200) {
      throw Exception('GetNearbyBookings failed: ${res.statusCode}');
    }

    final list = jsonDecode(res.body) as List;
    final bookings = list
        .map((e) => BookingPayload.fromJson(Map<String, dynamic>.from(e)))
        .toList();

    print('[REST] 📋 Nearby bookings: ${bookings.length}');
    return bookings;
  }

  // ── Disconnect ──────────────────────────────────────────────────────────────
  Future<void> disconnect() async {
    await _hub?.stop();
    print('[Hub] 🔌 Disconnected');
  }

  // ── Event Handlers (override in subclass or replace with callbacks) ─────────
  void _onBusActivated(ActiveBusPayload payload) {}
  void _onBusDeactivated(int busId) {}
  void _onBookingAdded(BookingPayload booking) {}
  void _onBookingStatusChanged(BookingStatusPayload payload) {}
}

// ─── Main — سيناريو كامل ──────────────────────────────────────────────────────
Future<void> main() async {
  final driver = DriverService(
    phone:    '0911111111',
    password: 'Password@123',
  );

  // 1. تسجيل الدخول
  await driver.login();

  // 2. جلب معلومات الباص
  await driver.loadBusInfo();

  // 3. الاتصال بالـ Hub
  await driver.connectHub();

  // 4. تفعيل الباص
  await driver.activateBus();

  // 5. إرسال الموقع
  await driver.updateLocation(33.9668, 36.6572);
  await Future.delayed(const Duration(seconds: 3));
  await driver.updateLocation(33.9675, 36.6585);

  // 6. جلب الحجوزات الحالية
  final bookings = await driver.getNearbyBookings();

  // 7. تأكيد أول حجز إن وجد
  if (bookings.isNotEmpty) {
    await driver.confirmBooking(bookings.first.id);
  }

  // 8. انتظار أحداث لايف
  print('\n[Main] ⏳ Listening for live events (30s)…\n');
  await Future.delayed(const Duration(seconds: 30));

  // 9. إيقاف الباص
  await driver.deactivateBus();

  // 10. قطع الاتصال
  await driver.disconnect();
}
