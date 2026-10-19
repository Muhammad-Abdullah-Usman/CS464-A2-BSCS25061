# CS464 Assignment 02 · Muhammad Abdullah · BSCS25061

- **Video:** [Google Drive gameplay video] (https://drive.google.com/file/d/1vtuv1Yqc9L65xxpc9rvM6YUzYUr2Chj-/view?usp=sharing)
- **Input:** Legacy Input with Active Input Handling set to Both
- **Ground check:** Contact normals; only contacts with normal.y > 0.5 count as ground
- **Camera:** Frame-rate-independent exponential smoothing with velocity look-ahead
- **Juice:** Landing squash using Ease.OutBack, triggered after a real landing

## Tuning

| Setting | Value | Why this number |
|---|---:|---|
| Speed | 5 m/s | Gives responsive movement while still allowing precise control on the narrow and elevated routes. At this speed, the 0.1 s coyote window represents 0.5 m of travel. |
| Jump height | 1.8 m | High enough for the existing platform and ledge challenges while remaining below the 3 m wall height. |
| Time to apex | 0.61 s | Follows from a 1.8 m jump under Unity gravity: launch speed is about 5.94 m/s, giving roughly 0.61 s to the apex. |
| Fall multiplier | 2.2× | Keeps the 1.8 m apex but shortens the fall to about 0.41 s, making the jump feel less floaty. |
| Coyote time | 0.10 s | At 5 m/s this permits a jump up to 0.5 m after leaving an edge, enough to forgive slightly late presses without making the jump obviously artificial. |
| Jump buffer | 0.12 s | Stores a press made shortly before landing so the jump fires immediately on contact instead of losing an early input. |
| Camera smoothing | Sharpness 5 | Exponential smoothing reaches about 90% of its target in roughly 0.46 s while behaving consistently at different frame rates. |
| Look-ahead | 0.35 s | At the maximum 5 m/s movement speed, the camera can aim about 1.75 m ahead of the player, showing more of the route being approached. |

## Four answers

### 1. The jump, in numbers

With `jumpHeight = 1.8 m` and gravity magnitude `g = 9.81 m/s²`:

`v0 = sqrt(2gh) = sqrt(2 × 9.81 × 1.8) = 5.94 m/s`

My Rigidbody mass is 1 kg, so the upward impulse is approximately:

`jumpForce = m × v0 = 1 × 5.94 = 5.94`

The time to the apex is:

`t = v0 / g = 5.94 / 9.81 = 0.61 s`

With a fall multiplier of `2.2`, the downward acceleration is about `2.2g`, so falling from the 1.8 m apex takes approximately `0.41 s`.

The whole airborne time is therefore roughly `1.02 s`. At the maximum horizontal speed of `5 m/s`, one jump can theoretically travel about `5.1 m` horizontally, so it can clear the widest approximately 3 m gaps in my levels.

### 2. Update and FixedUpdate

`Update()` reads the WASD axes and the one-frame Space press. It also handles the coyote-time and jump-buffer timestamps and builds the camera-relative movement direction.

`FixedUpdate()` applies that stored movement to `rb.linearVelocity`, executes a queued jump once, and adds the extra falling gravity.

If I moved `GetKeyDown` into `FixedUpdate`, presses could be missed because render frames and physics steps do not occur at the same rate. If I performed Rigidbody physics directly in `Update`, the movement would depend more on the rendering frame rate instead of Unity's fixed physics timing.

### 3. Vector3.Dot

My current ground check uses:
```csharp
if (contact.normal.y > 0.5f)
```

The equivalent dot product check would be:

```csharp
float upDot = Vector3.Dot(contact.normal, Vector3.up);
if (upDot > 0.5f)
{
    isGrounded = true;
}
```

A positive value means the contact surface faces upward. A value near zero represents a near-vertical wall, and a negative value represents a surface facing downward. Therefore > 0.5 accepts floor-like surfaces but rejects walls.

### 4. The Juice
My juice effect is a landing squash on the visual Body child while the Player's actual collider remains unchanged.
It uses Ease.OutBack, which overshoots slightly before settling at its target. I use Vector3.LerpUnclamped so that the overshoot is not clipped.
The squash lasts 0.18 s, which is:
0.18 × 60 = 10.8
or approximately 11 frames at 60 fps.
I chose a short duration so the landing is visibly acknowledged without delaying control. It only starts after the player has actually been airborne for more than 0.1 seconds, so simply standing on the floor does not repeatedly restart the effect.

### Levels
| Level | What the controller must do there | What is changed for this jump |
|---|---:|---|
| Level01 | Move accurately through the opening and across the narrow bridge route to the goal. | No geometry change was needed; the 5 m/s movement and controller fit the existing route. |
| Level02 | Handle the sloped split routes and choose between the shorter risky path and the longer safer route. | No change was needed after testing the 1.8 m jump and movement speed. |
| Level03 | Navigate the maze and tight turns while following the breadcrumb route. | No geometry change was required; camera-relative movement works with the maze layout. |
| Level04 | Climb and descend the rising platform/pyramid route using the jump consistently. | The existing platform spacing works with the 1.8 m jump, so no gap adjustment was required. |
| Level05 | Stay on the narrow elevated route around the central tower and reach the framed goal. | No geometry change was required after testing the controller on the narrow path. |