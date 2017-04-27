#!/usr/bin/env python

import sys
import time
import socket
import platform
import struct
from Crypto.PublicKey import RSA
from Crypto.Cipher import PKCS1_OAEP

def uptime():
  with open('/proc/uptime', 'r') as f:
    uptime_seconds = float(f.readline().split()[0])
  return int(uptime_seconds)

def ram_usage():
  mem_free = 0
  mem_buff = 0
  mem_cached = 0
  with open('/proc/meminfo', 'r') as f:
    for line in f.readlines():
      if line.startswith("MemFree"):
        mem_free = int(line.split()[1])
      elif line.startswith("Buffers"):
        mem_buffers = int(line.split()[1])
      elif line.startswith("Cached"):
        mem_cached = int(line.split()[1])
  return (mem_free + mem_buffers + mem_cached) / 1024

def cpu_usage():
  global cpu_work
  global cpu_total
  c = [0] * 7
  with open('/proc/stat', 'r') as f:
    c = map(int, f.readline().split()[1:])
  cpu_usage = 100 * (sum(c[:3]) - cpu_work) / (sum(c) - cpu_total)
  cpu_work = sum(c[:3])
  cpu_total = sum(c)
  return cpu_usage

def cpu_vendor():
  vendor = ""
  with open('/proc/cpuinfo', 'r') as f:
    for line in f.readlines():
      if line.startswith("vendor_id"):
        vendor = line.split(":")[1].strip()
        break
  return vendor

def cpu_model():
  model = ""
  with open('/proc/cpuinfo', 'r') as f:
    for line in f.readlines():
      if line.startswith("model name"):
        model = line.split(":")[1].strip()
        break
  return model


TCP_IP = sys.argv[1]
TCP_PORT = int(sys.argv[2])

s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.connect((TCP_IP, TCP_PORT))

key_len = struct.unpack("@i", s.recv(4))[0]
raw_key = s.recv(key_len)
key = RSA.importKey(raw_key)
cipher = PKCS1_OAEP.new(key)

cpu_work = 0
cpu_total = 0

hostname = platform.node()
system = platform.system()
vendor = cpu_vendor()
model = cpu_model()

while True:
  message = '''{
"<MachineName>k__BackingField": "%s"
,"<VendorId>k__BackingField": "%s"
,"<ModelName>k__BackingField": "%s"
,"<SystemType>k__BackingField": "%s"
,"<Uptime>k__BackingField": "%d"
,"<CpuLoad>k__BackingField": "%d"
,"<RamLoad>k__BackingField": "%d"}''' % (hostname, vendor, model, system, uptime(), cpu_usage(), ram_usage())
  enc_message = cipher.encrypt(message)
  s.send(struct.pack("@i", len(enc_message)))
  s.send(enc_message)
  time.sleep(1)
