import math
import sys

if len(sys.argv) > 1:
    try:
        A = float(sys.argv[1])
    except ValueError:
        A = None
else:
    A = None

while A is None or A == 0:
    try:
        A = float(input('Введите А: '))
        if A == 0:
            print('А не должно быть 0')
    except ValueError:
        print("A должно быть числом")
        A = None

if len(sys.argv) > 2:
    try:
        B = float(sys.argv[2])
    except ValueError:
        B = None
else:
    B = None

while B is None:
    try:
        B = float(input('Введите B: '))
    except ValueError:
        print("B должно быть числом")

if len(sys.argv) > 3:
    try:
        C = float(sys.argv[3])
    except ValueError:
        C = None
else:
    C = None

while C is None:
    try:
        C = float(input('Введите C: '))
    except ValueError:
        print("C должно быть числом")

D = B**2 - 4*A*C
roots = []
if D < 0:
    print('нет действительных корней')
elif D == 0:
    t = (-B)/(2*A)
    if t > 0:
        x1 = -math.sqrt(t)
        x2 = math.sqrt(t)
        roots.append(x1)
        roots.append(x2)
    elif t == 0:
        x = 0
        roots.append(x)
    else:
        print('Корней нет')
else:
    t1 = (-B + math.sqrt(D))/(2*A)
    if t1 < 0:
        pass
    elif t1 == 0:
        x = 0
        roots.append(x)
    else:
        x1 = math.sqrt(t1)
        x2 = -math.sqrt(t1)
        roots.append(x1)
        roots.append(x2)
    t2 = (-B - math.sqrt(D))/(2*A)
    if t2 < 0:
        pass
    elif t2 == 0:
        x = math.sqrt(t2)
        roots.append(x)
    else:
        x1 = math.sqrt(t2)
        x2 = -math.sqrt(t2)
        roots.append(x1)
        roots.append(x2)


if D == 0 or D > 0:
    print('Действительные корни:')
    for i, root in enumerate(roots, 1):
        print(f'x{i} = {root}')
