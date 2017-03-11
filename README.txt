Submitted by:
Nadav Sheffer 203653894

Spaceship #1: Snake
Really dumb brain that just moves in 90 degrees and shoots. This was used for
training.

Spaceship #2: Kamikaze
Tries to crash into the closest ships. Preferably with the shield on.
Raises the shield whenever another spaceship or shot is nearbye and constantly
targets the closest.
Issues:
- Calculations don't take into account screen edges
- Does not shoot since it will trigger it's own shield. Need to be able to
  recognize its own shots.
- Doesn't check if the target ship has a shield on.
- Tries to crash into other ships even when the shield is depleted instead of
  trying to escape and refresh. However, this makes the kamikaze effect of both
  ships dying which is not so bad.
