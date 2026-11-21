# MQTT contracts v1

Changes to these contracts are reviewed together with the backend and the simulator. JSON Schema draft 2020-12.

## Topics

Pattern: `v1/{locationId}/{deviceId}/{channel}`. `locationId` is a GUID; `deviceId` matches `^[A-Za-z0-9_-]{3,64}$` and equals the CN of the device certificate.

| Channel | Direction | QoS | Retained | Schema |
|---|---|---|---|---|
| `telemetry` | device -> cloud | 1 | no | telemetry.schema.json |
| `event` | device -> cloud | 1 | no | event.schema.json |
| `status` | device -> cloud | 1 | yes | status.schema.json |
| `cmd` | cloud -> device | 1 | no | command.schema.json |
| `cmd/ack` | device -> cloud | 1 | no | command-ack.schema.json |

## Broker ACL (Mosquitto, `use_identity_as_username true`, user = certificate CN)

Device (user = deviceId):
- write `v1/+/%u/telemetry`, `v1/+/%u/event`, `v1/+/%u/status`, `v1/+/%u/cmd/ack`
- read `v1/+/%u/cmd`

Backend (user = `backend`):
- read `v1/+/+/telemetry`, `v1/+/+/event`, `v1/+/+/status`, `v1/+/+/cmd/ack`
- write `v1/+/+/cmd`

The backend drops any message whose topic `locationId` does not match the Location of the Resource bound to that `deviceId`, and any payload whose `deviceId` differs from the topic.

## Rules

- Unknown fields are ignored by readers (forward compatible). Additive changes keep `schemaVersion`. A breaking change bumps `schemaVersion` and the topic prefix (`v2`).
- Timestamps are UTC RFC 3339 with milliseconds. Device clocks may drift; the backend also stores its own receive time and tolerates out-of-order messages.
- Telemetry idempotency key: `(deviceId, bootId, seq)`. `bootId` is a new UUID at every device start; `seq` starts at 0 for each boot and grows by 1 per telemetry message.
- Report interval defaults: 60 s idle, 10 s while running. `setReportInterval` changes both (5 to 600 s).
- Connect sequence: connect with last will on `status` = `{"state":"offline"}` (retained), then publish `{"state":"online", "bootId":..., "firmware":...}` (retained), then telemetry immediately.
- `status` retained payloads let the backend know the device state after its own restart.

## Commands

Every command carries `commandId` (UUID), `issuedAt`, `expiresAt` and optional `params`. The device ignores expired commands (acks `rejected`, reason `expired`), executes each `commandId` at most once (keep the last 50; a repeat is acked again with the original result without executing), and acks every command on `cmd/ack`.

| Command | params | Device behavior |
|---|---|---|
| `showCode` | `code`, `validUntil` | show the code on the display until `validUntil`; the device never generates or checks codes |
| `showMessage` | `text` (max 40 chars), `seconds` (1-60) | show a message |
| `unlock` | `displayName` (max 24) | unlock the door, show the name |
| `start` | `programId`, `minutes`, optional `programName` | start the cycle; telemetry phase becomes `running` |
| `stop` | - | stop the cycle, phase `idle` |
| `lock` | - | lock the door |
| `setReportInterval` | `idleSec`, `runningSec` | change reporting |

Backend delivery: ack timeout 10 s. `unlock`, `start`, `stop`, `lock`, `setReportInterval` are retried twice with the same `commandId`; then the session step fails with a problem response. `showCode` and `showMessage` are never retried.

## Device events

`event` carries `type`. v1 defines `buttonPressed` only. Faults travel in telemetry (`phase` = `fault` with `faultCode`).

## Machine cycle state (derived by Telemetry)

`phase` maps one to one: `idle`, `running`, `finished`, `fault`. `Unknown` is derived from `status` offline or no telemetry for 3 report intervals.

## Device behavior rules (v1)

1. `unlock` sets `locked` to false. It stays false until `lock`, `start` (sets it true) or the end of the cycle (`finished` sets it false).
2. Ack reasons use this vocabulary: `expired`, `busy`, `door_open`, `fault`, `invalid`.
   - `start` while running, or while finished with the door still closed: rejected `busy`.
   - `start` or `lock` with the door open: rejected `door_open`.
   - `unlock` while running: rejected `busy`.
   - `stop` while idle: `ok`.
   - `stop` during a fault: rejected `fault`. The backend treats this as expected, not as a failure.
3. A fault is cleared on the device only; there is no reset command. The phase returns to `idle` when it is cleared.
4. Besides the interval ticks, the device sends telemetry immediately on every phase change and after every executed command. `seq` continues normally.
5. `bootId` changes only when the device process starts. Reconnecting keeps `bootId` and `seq`; nothing is generated while offline, so `seq` has no gaps.
6. `remainingSec` and `start.minutes` are in device time (simulated seconds and minutes when a time scale is used). Report intervals are wall-clock. Time scaling exists only for demos.
7. A command with no readable `commandId` is dropped and logged, with no ack. A readable `commandId` with a payload that breaks the schema is acked `rejected` with reason `invalid`.
8. The dedupe cache (last 50 `commandId` values) may live in memory only; `expiresAt` bounds any replay.
9. `stop` while running or finished ends the cycle (phase `idle`) and sets `locked` to false.
10. `start` is checked in this order: `fault`, then `busy`, then `door_open`.
11. `unlock` is allowed in `idle`, `finished` and `fault`. `showCode` with a `validUntil` already in the past is acked `ok` and shows nothing.
12. Time is frozen during a fault. Clearing the fault goes to `idle` and drops program and step. `finished` reports `remainingSec` 0 and keeps `programId`; `idle` omits both.
13. The door opens by itself 30 device seconds after `finished` (configurable), so the backend sees `finished` for at least that long.
14. Metric units: `vibration` is a unitless index (0 to about 4), `currentA` is amperes, `temperatureC` is degrees Celsius. The backend does not threshold on metrics in v1; a fault comes only from phase `fault`.

Backend mapping of ack reasons to API problems: `busy` -> `machine-busy`; `fault` -> `machine-unavailable`; `door_open` -> `command-failed` with a detail asking to close the door; `expired` and `invalid` -> `command-failed`.

## Simulator identities

Generated devices use ids `sim-0001`, `sim-0002`, and so on, with certificate CN equal to the id. Certificate directory layout: `<dir>/ca.crt`, `<dir>/<deviceId>.crt`, `<dir>/<deviceId>.key`. The simulator takes `--cert-dir` and `--location-id`.
