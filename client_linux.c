#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <string.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <sys/sysinfo.h>
#include <netinet/in.h>
#include <netdb.h> 

int make_connection(int sockfd, const char *hostname, int port)
{
    struct hostent *server = gethostbyname(hostname);
    if (server == NULL) {
        fprintf(stderr,"ERROR, no such host\n");
        exit(0);
    }

    struct sockaddr_in serv_addr;
    bzero((char *) &serv_addr, sizeof(serv_addr));
    serv_addr.sin_family = AF_INET;
    bcopy((char *)server->h_addr, 
         (char *)&serv_addr.sin_addr.s_addr,
         server->h_length);
    serv_addr.sin_port = htons(port);
    
    while (connect(sockfd, (struct sockaddr *) &serv_addr, sizeof(serv_addr)) < 0) {
        puts("Cannot connect to server. Trying again...");
        sleep(5);
    }
}

int main(int argc, char *argv[])
{
    if (argc < 3) {
       fprintf(stderr,"usage %s hostname port\n", argv[0]);
       exit(0);
    }
    int sockfd = socket(AF_INET, SOCK_STREAM, 0);
    if (sockfd < 0) {
        puts("ERROR opening socket");
        exit(1);
    }
    make_connection(sockfd, argv[1], atoi(argv[2]));
    puts("connected!");
    char hostname[1024];
    hostname[1023] = '\0';
    gethostname(hostname, 1023);
    char os[1024] = "Linux";
    struct sysinfo info;
    int cpu_work = 0, cpu_total = 0;
    while(1) {
        int mem_free = 0, mem_buff = 0, mem_cached = 0;
        FILE *fd = fopen("/proc/meminfo", "r");
        char b1[256];
        char b2[256];
        int n;
        while (!mem_free || !mem_buff || !mem_cached) {
            fscanf(fd, "%s %d %s", b1, &n, b2);
            if (!strncmp(b1, "MemFree", 7)) {mem_free = n;}
            if (!strncmp(b1, "Buffers", 7)) {mem_buff = n;}
            if (!strncmp(b1, "Cached", 6)) {mem_cached = n;}
        }
        fclose(fd);

        fd = fopen("/proc/stat", "r");
        int c[7];
        fscanf(fd, "%s %d %d %d %d %d %d %d", b1, &c[0], &c[1], &c[2], &c[3], &c[4], &c[5], &c[6]);
        fclose(fd);
        int cpu_usage = 100 * (c[0] + c[1] + c[2] - cpu_work) /
                        (c[0] + c[1] + c[2] + c[3] + c[4] + c[5] + c[6] - cpu_total);
        cpu_work = c[0] + c[1] + c[2];
        cpu_total = c[0] + c[1] + c[2] + c[3] + c[4] + c[5] + c[6];

        sysinfo(&info);
        char buffer[1024];
        sprintf(buffer,
            "{"
            "\"<MachineName>k__BackingField\": \"%s\""
            ", \"<SystemType>k__BackingField\": \"%s\""
            ", \"<Uptime>k__BackingField\": \"%ld\""
            ", \"<CpuLoad>k__BackingField\": \"%d\""
            ", \"<RamLoad>k__BackingField\": \"%d\""
            "}", hostname, os, info.uptime, cpu_usage, (mem_free + mem_buff + mem_cached) / 1024);
        int length = /*htonl*/(strlen(buffer));
        n = write(sockfd, &length, 4);
        if (n < 0) {
            make_connection(sockfd, argv[1], atoi(argv[2]));
            continue;
        }
        n = write(sockfd, buffer, strlen(buffer));
        if (n < 0) {
            make_connection(sockfd, argv[1], atoi(argv[2]));
            continue;
        }
        sleep(1);
    }
    close(sockfd);
    return 0;
}