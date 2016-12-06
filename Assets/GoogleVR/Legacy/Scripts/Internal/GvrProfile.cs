﻿// Copyright 2014 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using UnityEngine;

/// @cond
/// Measurements of a particular phone in a particular VR viewer.
[System.Serializable]
public class GvrProfile {
  public GvrProfile Clone() {
    return new GvrProfile {
      screen = this.screen,
      viewer = this.viewer
    };
  }

  /// Information about the screen.  All distances are in meters, measured relative to how
  /// the phone is expected to be seated in the viewer, i.e. landscape orientation.
  [System.Serializable]
  public struct Screen {
    public float width;   // The long edge of the phone.
    public float height;  // The short edge of the phone.
    public float border;  // Distance from bottom of the phone to the bottom edge of screen.
  }

  /// Information about the lens placement in the viewer.  All distances are in meters.
  [System.Serializable]
  public struct Lenses {
    public float separation;     // Center to center.
    public float offset;         // Offset of lens center from top or bottom of viewer.
    public float screenDistance; // Distance from lens center to the phone screen.

    public int alignment;  // Determines whether lenses are placed relative to top, bottom or
                           // center.  It is actually a signum (-1, 0, +1) relating the scale of
                           // the offset's coordinates to the device coordinates.

    public const int AlignTop = -1;    // Offset is measured down from top of device.
    public const int AlignCenter = 0;  // Center alignment ignores offset, hence scale is zero.
    public const int AlignBottom = 1;  // Offset is measured up from bottom of device.
  }

  /// Information about the viewing angles through the lenses.  All angles in degrees, measured
  /// away from the optical axis, i.e. angles are all positive.  It is assumed that left and right
  /// eye FOVs are mirror images, so that both have the same inner and outer angles.  Angles do not
  /// need to account for the limits due to screen size.
  [System.Serializable]
  public struct MaxFOV {
    public float outer;  // Towards the side of the screen.
    public float inner;  // Towards the center line of the screen.
    public float upper;  // Towards the top of the screen.
    public float lower;  // Towards the bottom of the screen.
  }

  /// Information on how the lens distorts light rays.  Also used for the (approximate) inverse
  /// distortion.  Assumes a radially symmetric pincushion/barrel distortion model.
  [System.Serializable]
  public struct Distortion {
    private float[] coef;
    public float[] Coef {
      get {
        return coef;
      }
      set {
        if (value != null) {
          coef = (float[])value.Clone();
        } else {
          coef = null;
        }
      }
    }

    public float distort(float r) {
      float r2 = r * r;
      float ret = 0;
      for (int j=coef.Length-1; j>=0; j--) {
        ret = r2 * (ret + coef[j]);
      }
      return (ret + 1) * r;
    }

    public float distortInv(float radius) {
      // Secant method.
      float r0 = 0;
      float r1 = 1;
      float dr0 = radius - distort(r0);
      while (Mathf.Abs(r1 - r0) > 0.0001f) {
        float dr1 = radius - distort(r1);
        float r2 = r1 - dr1 * ((r1 - r0) / (dr1 - dr0));
        r0 = r1;
        r1 = r2;
        dr0 = dr1;
      }
      return r1;
    }
  }

  /// Information about a particular device, including specfications on its lenses, FOV,
  /// and distortion and inverse distortion coefficients.
  [System.Serializable]
  public struct Viewer {
    public Lenses lenses;
    public MaxFOV maxFOV;
    public Distortion distortion;
    public Distortion inverse;
  }

  /// Screen parameters of a Cardboard device.
  public Screen screen;

  /// Viewer parameters of a Cardboard device.
  public Viewer viewer;

  /// The vertical offset of the lens centers from the screen center.
  public float VerticalLensOffset {
    get {
      return (viewer.lenses.offset - screen.border - screen.height/2) * viewer.lenses.alignment;
    }
  }

  /// Some known screen profiles.
  public enum ScreenSizes {
    Nexus5,
    Nexus6,
    GalaxyS6,
    GalaxyNote4,
    LGG3,
    iPhone4,
    iPhone5,
    iPhone6,
    iPhone6p,
  };

  /// Parameters for a Nexus 5 device.
  public static readonly Screen Nexus5 = new Screen {
    width = 0.110f,
    height = 0.062f,
    border = 0.004f
  };

  /// Parameters for a Nexus 6 device.
  public static readonly Screen Nexus6 = new Screen {
    width = 0.133f,
    height = 0.074f,
    border = 0.004f
  };

  /// Parameters for a Galaxy S6 device.
  public static readonly Screen GalaxyS6 = new Screen {
    width = 0.114f,
    height = 0.0635f,
    border = 0.0035f
  };

  /// Parameters for a Galaxy Note4 device.
  public static readonly Screen GalaxyNote4 = new Screen {
    width = 0.125f,
    height = 0.0705f,
    border = 0.0045f
  };

  /// Parameters for a LG G3 device.
  public static readonly Screen LGG3 = new Screen {
    width = 0.121f,
    height = 0.068f,
    border = 0.003f
  };

  /// Parameters for an iPhone 4 device.
  public static readonly Screen iPhone4 = new Screen {
    width = 0.075f,
    height = 0.050f,
    border = 0.0045f
  };

  /// Parameters for an iPhone 5 device.
  public static readonly Screen iPhone5 = new Screen {
    width = 0.089f,
    height = 0.050f,
    border = 0.0045f
  };

  /// Parameters for an iPhone 6 device.
  public static readonly Screen iPhone6 = new Screen {
    width = 0.104f,
    height = 0.058f,
    border = 0.005f
  };

  /// Parameters for an iPhone 6p device.
  public static readonly Screen iPhone6p = new Screen {
    width = 0.112f,
    height = 0.068f,
    border = 0.005f
  };

  /// Some known Cardboard device profiles.
  public enum ViewerTypes {
    CardboardJun2014,
    CardboardMay2015,
    GoggleTechC1Glass,
  };

  /// Parameters for a Cardboard v1.
  public static readonly Viewer CardboardJun2014 = new Viewer {
    lenses = {
      separation = 0.060f,
      offset = 0.035f,
      screenDistance = 0.042f,
      alignment = Lenses.AlignBottom,
    },
    maxFOV = {
      outer = 40.0f,
      inner = 40.0f,
      upper = 40.0f,
      lower = 40.0f
    },
    distortion = {
      Coef = new [] { 0.441f, 0.156f },
    },
    inverse = ApproximateInverse(new [] { 0.441f, 0.156f })
  };

  /// Parameters for a Cardboard v2.
  public static readonly Viewer CardboardMay2015 = new Viewer {
    lenses = {
      separation = 0.064f,
      offset = 0.035f,
      screenDistance = 0.039f,
      alignment = Lenses.AlignBottom,
    },
    maxFOV = {
      outer = 60.0f,
      inner = 60.0f,
      upper = 60.0f,
      lower = 60.0f
    },
    distortion = {
      Coef = new [] { 0.34f, 0.55f },
    },
    inverse = ApproximateInverse(new [] { 0.34f, 0.55f })
  };

  /// Parameters for a Go4D C1-Glass.
  public static readonly Viewer GoggleTechC1Glass = new Viewer {
    lenses = {
      separation = 0.065f,
      offset = 0.036f,
      screenDistance = 0.058f,
      alignment = Lenses.AlignBottom,
    },
    maxFOV = {
      outer = 50.0f,
      inner = 50.0f,
      upper = 50.0f,
      lower = 50.0f
    },
    distortion = {
      Coef = new [] { 0.3f, 0 },
    },
    inverse = ApproximateInverse(new [] { 0.3f, 0 })
  };

  /// Nexus 5 in a Cardboard v1.
  public static readonly GvrProfile Default = new GvrProfile {
    screen = Nexus5,
    viewer = CardboardJun2014
  };

  /// Returns a profile with the given parameters.
  public static GvrProfile GetKnownProfile(ScreenSizes screenSize, ViewerTypes deviceType) {
    Screen screen;
    switch (screenSize) {
      case ScreenSizes.Nexus6:
        screen = Nexus6;
        break;
      case ScreenSizes.GalaxyS6:
        screen = GalaxyS6;
        break;
      case ScreenSizes.GalaxyNote4:
        screen = GalaxyNote4;
        break;
      case ScreenSizes.LGG3:
        screen = LGG3;
        break;
      case ScreenSizes.iPhone4:
        screen = iPhone4;
        break;
      case ScreenSizes.iPhone5:
        screen = iPhone5;
        break;
      case ScreenSizes.iPhone6:
        screen = iPhone6;
        break;
      case ScreenSizes.iPhone6p:
        screen = iPhone6p;
        break;
      default:
        screen = Nexus5;
        break;
    }
    Viewer device;
    switch (deviceType) {
      case ViewerTypes.CardboardMay2015:
        device = CardboardMay2015;
        break;
      case ViewerTypes.GoggleTechC1Glass:
        device = GoggleTechC1Glass;
        break;
      default:
        device = CardboardJun2014;
        break;
    }
    return new GvrProfile { screen = screen, viewer = device };
  }

  /// Calculates the tan-angles from the maximum FOV for the left eye for the
  /// current device and screen parameters.
  public void GetLeftEyeVisibleTanAngles(float[] result) {
    // Tan-angles from the max FOV.
    float fovLeft = Mathf.Tan(-viewer.maxFOV.outer * Mathf.Deg2Rad);
    float fovTop = Mathf.Tan(viewer.maxFOV.upper * Mathf.Deg2Rad);
    float fovRight = Mathf.Tan(viewer.maxFOV.inner * Mathf.Deg2Rad);
    float fovBottom = Mathf.Tan(-viewer.maxFOV.lower * Mathf.Deg2Rad);
    // Viewport size.
    float halfWidth = screen.width / 4;
    float halfHeight = screen.height / 2;
    // Viewport center, measured from left lens position.
    float centerX = viewer.lenses.separation / 2 - halfWidth;
    float centerY = -VerticalLensOffset;
    float centerZ = viewer.lenses.screenDistance;
    // Tan-angles of the viewport edges, as seen through the lens.
    float screenLeft = viewer.distortion.distort((centerX - halfWidth) / centerZ);
    float screenTop = viewer.distortion.distort((centerY + halfHeight) / centerZ);
    float screenRight = viewer.distortion.distort((centerX + halfWidth) / centerZ);
    float screenBottom = viewer.distortion.distort((centerY - halfHeight) / centerZ);
    // Compare the two sets of tan-angles and take the value closer to zero on each side.
    result[0] = Math.Max(fovLeft, screenLeft);
    result[1] = Math.Min(fovTop, screenTop);
    result[2] = Math.Min(fovRight, screenRight);
    result[3] = Math.Max(fovBottom, screenBottom);
  }

  /// Calculates the tan-angles from the maximum FOV for the left eye for the
  /// current device and screen parameters, assuming no lenses.
  public void GetLeftEyeNoLensTanAngles(float[] result) {
    // Tan-angles from the max FOV.
    float fovLeft = viewer.distortion.distortInv(Mathf.Tan(-viewer.maxFOV.outer * Mathf.Deg2Rad));
    float fovTop = viewer.distortion.distortInv(Mathf.Tan(viewer.maxFOV.upper * Mathf.Deg2Rad));
    float fovRight = viewer.distortion.distortInv(Mathf.Tan(viewer.maxFOV.inner * Mathf.Deg2Rad));
    float fovBottom = viewer.distortion.distortInv(Mathf.Tan(-viewer.maxFOV.lower * Mathf.Deg2Rad));
    // Viewport size.
    float halfWidth = screen.width / 4;
    float halfHeight = screen.height / 2;
    // Viewport center, measured from left lens position.
    float centerX = viewer.lenses.separation / 2 - halfWidth;
    float centerY = -VerticalLensOffset;
    float centerZ = viewer.lenses.screenDistance;
    // Tan-angles of the viewport edges, as seen through the lens.
    float screenLeft = (centerX - halfWidth) / centerZ;
    float screenTop = (centerY + halfHeight) / centerZ;
    float screenRight = (centerX + halfWidth) / centerZ;
    float screenBottom = (centerY - halfHeight) / centerZ;
    // Compare the two sets of tan-angles and take the value closer to zero on each side.
    result[0] = Math.Max(fovLeft, screenLeft);
    result[1] = Math.Min(fovTop, screenTop);
    result[2] = Math.Min(fovRight, screenRight);
    result[3] = Math.Max(fovBottom, screenBottom);
  }

  /// Calculates the screen rectangle visible from the left eye for the
  /// current device and screen parameters.
  public Rect GetLeftEyeVisibleScreenRect(float[] undistortedFrustum) {
    float dist = viewer.lenses.screenDistance;
    float eyeX = (screen.width - viewer.lenses.separation) / 2;
    float eyeY = VerticalLensOffset + screen.height / 2;
    float left = (undistortedFrustum[0] * dist + eyeX) / screen.width;
    float top = (undistortedFrustum[1] * dist + eyeY) / screen.height;
    float right = (undistortedFrustum[2] * dist + eyeX) / screen.width;
    float bottom = (undistortedFrustum[3] * dist + eyeY) / screen.height;
    return new Rect(left, bottom, right - left, top - bottom);
  }

  public static float GetMaxRadius(float[] tanAngleRect) {
    float x = Mathf.Max(Mathf.Abs(tanAngleRect[0]), Mathf.Abs(tanAngleRect[2]));
    float y = Mathf.Max(Mathf.Abs(tanAngleRect[1]), Mathf.Abs(tanAngleRect[3]));
    return Mathf.Sqrt(x * x + y * y);
  }

  // Solves a small linear equation via destructive gaussian
  // elimination and back substitution.  This isn't generic numeric
  // code, it's just a quick hack to work with the generally
  // well-behaved symmetric matrices for least-squares fitting.
  // Not intended for reuse.
  //
  // @param a Input positive definite symmetrical matrix. Destroyed
  //     during calculation.
  // @param y Input right-hand-side values. Destroyed during calculation.
  // @return Resulting x value vector.
  //
  private static double[] solveLinear(double[,] a, double[] y) {
    int n = a.GetLength(0);

    // Gaussian elimination (no row exchange) to triangular matrix.
    // The input matrix is a A^T A product which should be a positive
    // definite symmetrical matrix, and if I remember my linear
    // algebra right this implies that the pivots will be nonzero and
    // calculations sufficiently accurate without needing row
    // exchange.
    for (int j = 0; j < n - 1; ++j) {
      for (int k = j + 1; k < n; ++k) {
        double p = a[k, j] / a[j, j];
        for (int i = j + 1; i < n; ++i) {
          a[k, i] -= p * a[j, i];
        }
        y[k] -= p * y[j];
      }
    }
    // From this point on, only the matrix elements a[j][i] with i>=j are
    // valid. The elimination doesn't fill in eliminated 0 values.

    double[] x = new double[n];

    // Back substitution.
    for (int j = n - 1; j >= 0; --j) {
      double v = y[j];
      for (int i = j + 1; i < n; ++i) {
        v -= a[j, i] * x[i];
      }
      x[j] = v / a[j, j];
    }

    return x;
  }

  // Solves a least-squares matrix equation.  Given the equation A * x = y, calculate the
  // least-square fit x = inverse(A * transpose(A)) * transpose(A) * y.  The way this works
  // is that, while A is typically not a square matrix (and hence not invertible), A * transpose(A)
  // is always square.  That is:
  //   A * x = y
  //   transpose(A) * (A * x) = transpose(A) * y   <- multiply both sides by transpose(A)
  //   (transpose(A) * A) * x = transpose(A) * y   <- associativity
  //   x = inverse(transpose(A) * A) * transpose(A) * y  <- solve for x
  // Matrix A's row count (first index) must match y's value count.  A's column count (second index)
  // determines the length of the result vector x.
  private static double[] solveLeastSquares(double[,] matA, double[] vecY) {
    int numSamples = matA.GetLength(0);
    int numCoefficients = matA.GetLength(1);
    if (numSamples != vecY.Length) {
      Debug.LogError("Matrix / vector dimension mismatch");
      return null;
    }

    // Calculate transpose(A) * A
    double[,] matATA = new double[numCoefficients, numCoefficients];
    for (int k = 0; k < numCoefficients; ++k) {
      for (int j = 0; j < numCoefficients; ++j) {
        double sum = 0.0;
        for (int i = 0; i < numSamples; ++i) {
          sum += matA[i, j] * matA[i, k];
        }
        matATA[j, k] = sum;
      }
    }

    // Calculate transpose(A) * y
    double[] vecATY = new double[numCoefficients];
    for (int j = 0; j < numCoefficients; ++j) {
      double sum = 0.0;
      for (int i = 0; i < numSamples; ++i) {
        sum += matA[i, j] * vecY[i];
      }
      vecATY[j] = sum;
    }

    // Now solve (A * transpose(A)) * x = transpose(A) * y.
    return solveLinear(matATA, vecATY);
  }

  /// Calculates an approximate inverse to the given radial distortion parameters.
  public static Distortion ApproximateInverse(float[] coef, float maxRadius = 1,
                                              int numSamples = 100) {
    return ApproximateInverse(new Distortion { Coef=coef }, maxRadius, numSamples);
  }

  /// Calculates an approximate inverse to the given radial distortion parameters.
  public static Distortion ApproximateInverse(Distortion distort, float maxRadius = 1,
                                              int numSamples = 100) {
    const int numCoefficients = 6;

    // R + K1*R^3 + K2*R^5 = r, with R = rp = distort(r)
    // Repeating for numSamples:
    //   [ R0^3, R0^5 ] * [ K1 ] = [ r0 - R0 ]
    //   [ R1^3, R1^5 ]   [ K2 ]   [ r1 - R1 ]
    //   [ R2^3, R2^5 ]            [ r2 - R2 ]
    //   [ etc... ]                [ etc... ]
    // That is:
    //   matA * [K1, K2] = y
    // Solve:
    //   [K1, K2] = inverse(transpose(matA) * matA) * transpose(matA) * y
    double[,] matA = new double[numSamples, numCoefficients];
    double[] vecY = new double[numSamples];
    for (int i = 0; i < numSamples; ++i) {
      float r = maxRadius * (i + 1) / (float) numSamples;
      double rp = distort.distort(r);
      double v = rp;
      for (int j = 0; j < numCoefficients; ++j) {
        v *= rp * rp;
        matA[i, j] = v;
      }
      vecY[i] = r - rp;
    }
    double[] vecK = solveLeastSquares(matA, vecY);
    // Convert to float for use in a fresh Distortion object.
    float[] coefficients = new float[vecK.Length];
    for (int i = 0; i < vecK.Length; ++i) {
      coefficients[i] = (float) vecK[i];
    }
    return new Distortion { Coef = coefficients };
  }
}
/// @endcond
