__kernel void update_particles(__global float2* positions, __global float2* velocities, __global float* lifespans, const float dt) {
    int i = get_global_id(0);
    if (lifespans[i] > 0.0f) {
        positions[i].x += velocities[i].x * dt;
        positions[i].y += velocities[i].y * dt;
        lifespans[i] -= dt;
    }
}
