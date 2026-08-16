# -*- coding: utf-8 -*-
"""
Level Solver - Backtracking algorithm to verify Level Solvability in BlockFood.
Run: python level_solver.py
"""

def solve_level(grid_size, pieces, customers):
    W, H = grid_size
    grid = [[None for _ in range(H)] for _ in range(W)]
    
    def can_place(shape, origin_x, origin_y):
        for dx, dy in shape:
            x, y = origin_x + dx, origin_y + dy
            if x < 0 or x >= W or y < 0 or y >= H:
                return False
            if grid[x][y] is not None:
                return False
        return True
        
    def place(shape, origin_x, origin_y, piece_idx):
        for dx, dy in shape:
            grid[origin_x + dx][origin_y + dy] = piece_idx
            
    def unplace(shape, origin_x, origin_y):
        for dx, dy in shape:
            grid[origin_x + dx][origin_y + dy] = None
            
    solutions = []
    
    def backtrack(piece_idx, placements):
        if piece_idx == len(pieces):
            for cust_cells, req, cust_name in customers:
                touched_pieces = set()
                for cx, cy in cust_cells:
                    p_id = grid[cx][cy]
                    if p_id is not None:
                        touched_pieces.add(p_id)
                
                total_flavors = {}
                for p_id in touched_pieces:
                    p_flavors = pieces[p_id][1]
                    for flv, cnt in p_flavors.items():
                        total_flavors[flv] = total_flavors.get(flv, 0) + cnt
                
                for flv, req_cnt in req.items():
                    if total_flavors.get(flv, 0) < req_cnt:
                        return False
            
            solutions.append(list(placements))
            return True
            
        shape, flavors, name = pieces[piece_idx]
        found = False
        for x in range(W):
            for y in range(H):
                if can_place(shape, x, y):
                    place(shape, x, y, piece_idx)
                    placements.append((piece_idx, name, x, y))
                    if backtrack(piece_idx + 1, placements):
                        found = True
                    placements.pop()
                    unplace(shape, x, y)
        return found

    backtrack(0, [])
    return solutions

if __name__ == "__main__":
    print("==================================================")
    print("  BLOCKFOOD LEVEL VERIFICATION (BACKTRACKING)     ")
    print("==================================================")

    # --- LEVEL 2 ---
    pieces_lv2 = [
        ([(0,0), (1,0), (2,0)], {'spicy': 1, 'sweet': 1}, 'Kimbab_1 (Slot 0)'),
        ([(0,0), (1,0), (0,1), (1,1)], {'sour': 1, 'salty': 1}, 'GaChien_1 (Slot 1)'),
        ([(0,0), (1,0), (2,0)], {'umami': 1, 'spicy': 1}, 'Kimbab_2 (Slot 2)'),
    ]
    customers_lv2 = [
        ([(x, y) for x in [0, 1] for y in range(4)], {'sour': 1, 'salty': 1, 'spicy': 1}, 'Customer 1 (Left)'),
        ([(x, y) for x in [2, 3] for y in range(4)], {'sweet': 1, 'umami': 1, 'spicy': 1}, 'Customer 2 (Right)'),
    ]
    sols2 = solve_level((4, 4), pieces_lv2, customers_lv2)
    print(f"\n[Level 2] Valid Solutions Count: {len(sols2)}")
    print(f" -> Sample Solution: {sols2[0]}")

    # --- LEVEL 3 ---
    customers_lv3 = [
        ([(0,2),(0,3),(1,2),(1,3)], {'spicy': 1, 'sweet': 1}, 'Customer 1 (Top-Left)'),
        ([(0,0),(1,0),(2,0),(3,0)], {'salty': 1, 'umami': 1}, 'Customer 2 (Bottom)'),
        ([(2,2),(2,3),(3,2),(3,3)], {'sour': 1, 'buttery': 1}, 'Customer 3 (Top-Right)'),
    ]
    pieces_lv3 = [
        ([(0,0), (1,0), (2,0)], {'salty': 1, 'umami': 1}, 'Kimbab (Slot 0)'),
        ([(0,0), (1,0), (0,1), (1,1)], {'spicy': 1, 'sweet': 1}, 'GaChien 1 (Slot 1)'),
        ([(0,0), (1,0), (0,1), (1,1)], {'sour': 1, 'buttery': 1}, 'GaChien 2 (Slot 2)'),
    ]
    sols3 = solve_level((4, 4), pieces_lv3, customers_lv3)
    print(f"\n[Level 3] Valid Solutions Count: {len(sols3)}")
    print(f" -> Sample Solution: {sols3[0]}")

    # --- LEVEL 4 ---
    customers_lv4 = [
        ([(0,2),(0,3),(1,2),(1,3)], {'spicy': 1, 'salty': 1}, 'Customer 1 (Top-Left)'),
        ([(2,2),(2,3),(3,2),(3,3)], {'sour': 1, 'sweet': 1}, 'Customer 2 (Top-Right)'),
        ([(0,0),(0,1),(1,0),(1,1)], {'umami': 1, 'spicy': 1}, 'Customer 3 (Bottom-Left)'),
        ([(2,0),(2,1),(3,0),(3,1)], {'buttery': 1, 'salty': 1}, 'Customer 4 (Bottom-Right)'),
    ]
    pieces_lv4 = [
        ([(0,0), (1,0), (0,1), (1,1)], {'spicy': 1, 'salty': 1}, 'GaChien TL (Slot 0)'),
        ([(0,0), (1,0), (0,1), (1,1)], {'sour': 1, 'sweet': 1}, 'GaChien TR (Slot 1)'),
        ([(0,0), (1,0), (0,1), (1,1)], {'umami': 1, 'spicy': 1}, 'GaChien BL (Slot 2)'),
        ([(0,0), (1,0), (0,1), (1,1)], {'buttery': 1, 'salty': 1}, 'GaChien BR (Slot 3)'),
    ]
    sols4 = solve_level((4, 4), pieces_lv4, customers_lv4)
    print(f"\n[Level 4] Valid Solutions Count: {len(sols4)}")
    print(f" -> Sample Solution: {sols4[0]}")
    print("\nAll 3 levels verified 100% SOLVABLE with Backtracking!")
