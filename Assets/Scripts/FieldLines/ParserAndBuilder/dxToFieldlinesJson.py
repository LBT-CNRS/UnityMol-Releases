# Benoist Laurent a contribué à niquer mon code le 16 janvier 2019... Merci Benoist

import argparse
import numpy as np
import sys

import MDAnalysis as mda
from gridData import Grid

from math import floor
from math import sqrt

import time

# def main():

#     g = Grid(sys.argv[1])
#     print("Read ! ", g)


# if __name__== "__main__":
#   main()


# space to grid
def s2g(pos3d, dx, origin, dim):
    i = floor((max(0.0, pos3d[0] - origin[0]) / dx[0]))
    j = floor((max(0.0, pos3d[1] - origin[1]) / dx[1]))
    k = floor((max(0.0, pos3d[2] - origin[2]) / dx[2]))
    i = min(i, dim[0] - 1)
    j = min(j, dim[1] - 1)
    k = min(k, dim[2] - 1)
    return [i, j, k]

# grid to space


def g2s(ijk, origin, dx):
    x = origin[0] + ijk[0] * dx[0]
    y = origin[1] + ijk[1] * dx[1]
    z = origin[2] + ijk[2] * dx[2]
    return [x, y, z]

# to know if the trajectory go out of the grid3D


def isInBox(arraynp, dx, dim, origin):
    if(arraynp[0] > dim[0]*dx[0]):
        return False
    if(arraynp[0] < origin[0]):
        return False
    if(arraynp[1] > dim[1]*dx[1]):
        return False
    if(arraynp[1] < origin[1]):
        return False
    if(arraynp[2] > dim[2]*dx[2]):
        return False
    if(arraynp[2] < origin[2]):
        return False
    return True


def getInitPos(id, g):
    res = []
    for i in id:
        res.append([g2s(i, g.origin, g.delta)])
    return res


def computeGrad(grid):
    Z = grid.shape[2]
    Y = grid.shape[1]
    X = grid.shape[0]

    xsinv = 1.0
    if (X > 1):
        xsinv = 1.0 / (g.delta[0] - 1.0)

    ysinv = 1.0
    if (Y > 1):
        ysinv = 1.0 / (g.delta[1] - 1.0)

    zsinv = 1.0
    if (Z > 1):
        zsinv = 1.0 / (g.delta[2] - 1.0)

    xs = -0.5 * xsinv
    ys = -0.5 * ysinv
    zs = -0.5 * zsinv

    grad = np.empty((X, Y, Z, 3))

    for x in range(X):
        xm = np.clip(x - 1, 0, X-1)
        xp = np.clip(x + 1, 0, X-1)
        for y in range(Y):
            ym = np.clip(y - 1, 0, Y-1)
            yp = np.clip(y + 1, 0, Y-1)
            for z in range(Z):
                zm = np.clip(z - 1, 0, Z-1)
                zp = np.clip(z + 1, 0, Z-1)

                grad[x][y][z][0] = (grid[xp][y][z] - grid[xm][y][z]) * xs
                grad[x][y][z][1] = (grid[x][yp][z] - grid[x][ym][z]) * ys
                grad[x][y][z][2] = (grid[x][y][zp] - grid[x][y][zm]) * zs

    return grad


def sqrMagn(v):
    return np.sum(np.square(v))


def getSeeds(grad, gSize, gThreshold):
    minGrad = gThreshold * 0.5
    maxGrad = gThreshold * 1.5
    minGrad2 = minGrad * minGrad
    maxGrad2 = maxGrad * maxGrad

    res = []

    for x in range(gSize[0]):
        for y in range(gSize[1]):
            for z in range(gSize[2]):
                sqm = sqrMagn(grad[x][y][z])
                if sqm >= minGrad2 and sqm <= maxGrad2:
                    res.append([x, y, z])

    return res


def computeFL(idCells, g, Niter, minLength = 10.0, maxLength = 50.0):
    fieldlines = getInitPos(idCells, g)

    minGradMag = 0.0001
    maxGradMag = 5.0

    mincelllen = np.min(g.delta)
    delta = 0.25 * mincelllen;

    #TODO parallelize this
    for j in range(len(idCells)):
        lengthPerL = 0.0
        for it in range(1, Niter+1):
            prevP = fieldlines[j][it - 1]
            # Compute new cell index based on previous space position
            ijk = s2g(prevP, g.delta, g.origin, g.grid.shape)
            value = grad[ijk[0], ijk[1], ijk[2]]
            norm = np.sqrt(sqrMagn(value))

            if norm < minGradMag or norm > maxGradMag:
                break

            newP = prevP + (value * delta / norm)
            lengthPerL += delta

            if isInBox(newP, g.delta, g.grid.shape, g.origin):
                fieldlines[j].append(newP)
            else:
                break

        if lengthPerL > maxLength or lengthPerL < minLength:
            fieldlines[j] = []

    return fieldlines


def writeToJSONfile(fileName, fieldlinesPerParticle):
    with open(fileName, "w") as f:
        f.write("{\n")
        idP = 1
        for i, val in enumerate(fieldlinesPerParticle):
            if len(val) > 1:
                f.write("\"{}\":[".format(idP))
                for j, vec3 in enumerate(val):
                    if(j < len(val)-1):
                        f.write("{},".format('[{:s}]'.format(
                            ', '.join(['{:.4f}'.format(x) for x in vec3]))))
                    else:
                        f.write("{}".format('[{:s}]'.format(
                            ', '.join(['{:.4f}'.format(x) for x in vec3]))))
                if(i < len(fieldlinesPerParticle)-1):
                    f.write("],\n")
                else:
                    f.write("]\n")
                idP += 1

        f.write("}\n")
        if idP == 1:
            print("Warning, no fieldline written")


if len(sys.argv) != 3:
    print("Usage : input.dx output.json")
    exit(-1)

g = Grid(sys.argv[1])


grad = computeGrad(g.grid)
# gradnp = np.gradient(g.grid)
# grad = np.stack((gradnp[0], gradnp[1], gradnp[2]), axis=3)




# Nparticles = 100
Niter = 500

# N3 = np.sum(g.grid.shape)
# Nx3 = g.grid.shape[0] * g.grid.shape[1] * g.grid.shape[2]
# boolMask = np.zeros(g.grid.shape, dtype=bool)


# Choose starting cell ids
# if Nparticles == Nx3:
#     idCells = np.stack((Xs, Ys, Zs), axis = 1)
# else:

# Xs = np.random.randint(g.grid.shape[0], size=Nparticles)
# Ys = np.random.randint(g.grid.shape[1], size=Nparticles)
# Zs = np.random.randint(g.grid.shape[2], size=Nparticles)
# idCells = np.stack((Xs, Ys, Zs), axis=1)
# idCells = idCells.tolist()

gradMagn = 1.8

time1 = time.time()

idCells = getSeeds(grad, g.grid.shape, gradMagn)

fieldlinesPerParticle = computeFL(idCells, g, Niter)

time2 = time.time()

print('computeFL took {:.3f} ms'.format((time2-time1)*1000.0))

# write to file
fileName = sys.argv[2]

writeToJSONfile(fileName, fieldlinesPerParticle)
