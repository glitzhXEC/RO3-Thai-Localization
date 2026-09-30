"""Read the complete English-only snapshot, stored in bounded TSV parts."""
import csv
from pathlib import Path

def read_source(root: Path):
    folder=root/'translations/source'
    parts=sorted(folder.glob('english-part-*.tsv'))
    if not parts: parts=[folder/'english.tsv']
    rows=[]
    for part in parts:
        with part.open(encoding='utf-8',newline='') as f:
            reader=csv.DictReader(f,delimiter='\t')
            assert reader.fieldnames==['ID','English'], 'Unexpected source schema'
            rows.extend(reader)
    assert len({r['ID'] for r in rows})==len(rows), 'Duplicate source IDs'
    return rows
