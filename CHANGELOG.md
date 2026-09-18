# Changelog

## 2.0.0

- **Day and night are now really controlled separately.** 1.x only summed both
  values into `EnvMan.m_dayLengthSec`, which is the *total* cycle — the day/night
  split stayed at vanilla's fixed 70/30 because it lives in the boundaries of
  `EnvMan.RescaleDayFraction`, not in a field. Those boundaries are now derived
  from the configured proportion.
- Sleeping wakes you at the configured sunrise: `GetMorningStartSec` no longer
  uses the hardcoded `0.15`.
- New `Tempo.Enabled` toggle to hand the cycle back to the game (or to another mod).
- Detects **ZenWorldSettings** and states in the log which mod controls the cycle.
- Config changes apply immediately, with no restart.
- Defaults changed to 1260s day / 540s night, which reproduces vanilla exactly.
- Minimum length lowered from 100s to 10s, for testing.

# 🧾 CHANGELOG

## v1.0.0
- Initial release.
- Added configurable day and night durations.
- Full server sync with Jötunn.
