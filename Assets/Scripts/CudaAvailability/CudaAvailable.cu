#include <cuda_runtime.h>
//nvcc CudaAvailable.cu --shared -o CudaAvailability.dll

//
// Linearize OS specific macros
//
#if defined(__unix__) || defined(__linux__) || defined(__APPLE__) || defined(__MACH__)
#define OS_UNIX
#endif

#if defined(__APPLE__) || defined(__MACH__)
#define OS_OSX
#endif

#if defined(_MSC_VER) || defined(_WIN32) || defined(__CYGWIN__)
#define OS_WINDOWS
#endif

//
// API export macro
//
#if defined(OS_OSX)
#define API __attribute__((visibility("default")))
#elif defined(OS_WINDOWS)
#define API __declspec(dllexport)
#else
#define API
#endif

struct GpuCap
{
    bool QueryFailed;           // True on error
    int  DeviceCount;           // Number of CUDA devices found
    int  StrongestDeviceId;     // ID of best CUDA device
    int  ComputeCapabilityMajor; // Major compute capability (of best device)
    int  ComputeCapabilityMinor; // Minor compute capability
};





extern "C" {
    API bool isCudaAvailable() {
        GpuCap gpu;
        gpu.QueryFailed = false;
        gpu.StrongestDeviceId = -1;
        gpu.ComputeCapabilityMajor = -1;
        gpu.ComputeCapabilityMinor = -1;

        cudaError_t error_id = cudaGetDeviceCount(&gpu.DeviceCount);
        if (error_id != cudaSuccess)
        {
            gpu.QueryFailed = true;
            gpu.DeviceCount = 0;
            return false;
        }
        if (gpu.DeviceCount == 0)
            return false;

        return true;
    }
}
